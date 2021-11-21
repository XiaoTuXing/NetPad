using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaEdit;
using NetPad.UI.TextEditing;
using NetPad.ViewModels.Scripts;
using OmniSharp;
using ReactiveUI;

namespace NetPad.Views.Scripts
{
    public class ScriptView : ReactiveUserControl<ScriptViewModel>
    {
        private readonly TextEditorConfigurator _textEditorConfigurator;

        public ScriptView()
        {
        }

        public ScriptView(IOmniSharpServer omniSharpServer) : this()
        {
            InitializeComponent();

            // Needed so WhenActivated is called on viewmodel
            this.WhenActivated(disposables => { });

            _textEditorConfigurator = new TextEditorConfigurator(this.FindControl<TextEditor>("Editor"), omniSharpServer);
            _textEditorConfigurator.Setup();

            AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                var textEditor = _textEditorConfigurator.TextEditor;
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) textEditor.FontSize++;
                else textEditor.FontSize = textEditor.FontSize > 1 ? textEditor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnInitialized()
        {
            if (_textEditorConfigurator != null)
            {
                _textEditorConfigurator.TextEditor.Text = ViewModel!.ScriptEnvironment.Script.Code;
                _textEditorConfigurator.TextEditor.TextChanged += (sender,  args) =>
                    ViewModel!.Code = _textEditorConfigurator.TextEditor.Text;
                // _textEditorConfigurator.TextEditor.TextArea.TextEntered += (_, args) =>
                //     ViewModel!.Code = _textEditorConfigurator.TextEditor.Text ?? string.Empty;
            }
        }
    }
}
