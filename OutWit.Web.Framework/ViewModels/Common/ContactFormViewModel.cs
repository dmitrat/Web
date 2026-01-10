using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Components.Common;
using OutWit.Web.Framework.Configuration;
using OutWit.Web.Framework.Models;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Common;

public class ContactFormViewModel : ViewModelBase
{
    #region Fields

    private string m_turnstileToken = string.Empty;
    
    private DotNetObjectReference<ContactFormViewModel>? m_dotNetRef;

    #endregion

    #region Functions

    protected async Task HandleSubmit()
    {
        if (IsSubmitting) return;
        
        // Check honeypot (bot protection)
        if (!string.IsNullOrEmpty(Honeypot))
        {
            Submitted = true; // Pretend success for bots
            return;
        }

        // Validate Turnstile if required
        if (RequiresTurnstile && !TurnstileVerified)
        {
            Error = "Please complete the security verification.";
            return;
        }

        IsSubmitting = true;
        Error = string.Empty;

        try
        {
            var payload = new
            {
                name = Name,
                email = Email,
                message = Message,
                messageType = string.IsNullOrEmpty(MessageType) 
                    ? null 
                    : MessageType,
                formId = Config?.Contact.FormId ?? "default",
                turnstileToken = m_turnstileToken
            };

            var response = await HttpClient.PostAsJsonAsync(Config?.Contact.ApiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                Submitted = true;
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<ContactResponse>();
                Error = result?.Error ?? "Failed to send message. Please try again.";
            }
        }
        catch (Exception)
        {
            Error = "Failed to send message. Please try again later.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    #endregion

    #region Event Handlers

    protected override async Task OnInitializedAsync()
    {
        Config = await ConfigService.GetConfigAsync();
        TurnstileSiteKey = Config.Contact.TurnstileSiteKey ?? string.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(TurnstileSiteKey))
        {
            m_dotNetRef = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("renderTurnstile", m_dotNetRef, TurnstileSiteKey, "turnstile-container");
        }
    }

    [JSInvokable]
    public void OnTurnstileSuccess(string token)
    {
        m_turnstileToken = token;
        TurnstileVerified = true;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnTurnstileError()
    {
        TurnstileVerified = false;
        Error = "Security verification failed. Please refresh the page.";
        StateHasChanged();
    }

    [JSInvokable]
    public void OnTurnstileExpired()
    {
        TurnstileVerified = false;
        m_turnstileToken = string.Empty;
        StateHasChanged();
    }

    #endregion

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        m_dotNetRef?.Dispose();
        base.Dispose(disposing);
    }

    #endregion

    #region Properties

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Honeypot { get; set; } = string.Empty;
    
    
    public string TurnstileSiteKey { get; private set; } = string.Empty;
    public string Error { get; private set; } = string.Empty;
    public bool IsSubmitting { get; private set; }
    public bool Submitted { get; private set; }
    public bool TurnstileVerified { get; private set; }
    public bool RequiresTurnstile => !string.IsNullOrEmpty(TurnstileSiteKey);

    public SiteConfig? Config { get; private set; }

    #endregion

    #region Injected Dependencies

    [Inject]
    public ConfigService ConfigService { get; private set; } = null!;
    
    [Inject]
    public HttpClient HttpClient { get; private set; } = null!;
    
    [Inject]
    public IJSRuntime JsRuntime { get; private set; } = null!;

    #endregion
}