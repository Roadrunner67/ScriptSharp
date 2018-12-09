using ActiproSoftware.Text;
using ActiproSoftware.Text.Languages.CSharp.Implementation;
using ActiproSoftware.Text.Languages.DotNet;
using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.LLParser;
using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfScriptEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // A project assembly (similar to a Visual Studio project) contains source files and assembly references for reflection
        private IProjectAssembly projectAssembly;
        private bool hasPendingParseData;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes an instance of the <c>MainControl</c> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Set the header and footer on the fragment editor's document
            fragmentEditor.Document.SetHeaderAndFooterText(headerEditor.Document.CurrentSnapshot.Text, footerEditor.Document.CurrentSnapshot.Text);

            //
            // NOTE: Make sure that you've read through the add-on language's 'Getting Started' topic
            //   since it tells you how to set up an ambient parse request dispatcher and an ambient
            //   code repository within your application OnStartup code, and add related cleanup in your
            //   application OnExit code.  These steps are essential to having the add-on perform well.
            //

            // Initialize the project assembly (enables support for automated IntelliPrompt features)
            projectAssembly = new CSharpProjectAssembly("SampleBrowser");
            var assemblyLoader = new BackgroundWorker();
            assemblyLoader.DoWork += DotNetProjectAssemblyReferenceLoader;
            assemblyLoader.RunWorkerAsync();

            // Load the .NET Languages Add-on C# language and register the project assembly on it
            var language = new CSharpSyntaxLanguage();
            language.RegisterProjectAssembly(projectAssembly);
            fragmentEditor.Document.Language = language;

            // Create a parser-less C# language for the header/footer editors
            var parserlessLanguage = new CSharpSyntaxLanguage();
            parserlessLanguage.UnregisterParser();
            headerEditor.Document.Language = parserlessLanguage;
            footerEditor.Document.Language = parserlessLanguage;
        }

        private void DotNetProjectAssemblyReferenceLoader(object sender, DoWorkEventArgs e)
        {
            // Add some common assemblies for reflection (any custom assemblies could be added using various Add overloads instead)
            projectAssembly.AssemblyReferences.AddMsCorLib();
            projectAssembly.AssemblyReferences.Add("System");
            projectAssembly.AssemblyReferences.Add("System.Core");
            projectAssembly.AssemblyReferences.Add("System.Xml");
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // NON-PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AppendMessage(string message)
        {
            //((ScriptingViewModel)DataContext).AppendMessage(message);
        }

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnToggleEditModeButtonClick(object sender, RoutedEventArgs e)
        {
            bool fragmentEditorActive = fragmentEditor.Document.IsReadOnly;

            headerEditor.Document.IsReadOnly = fragmentEditorActive;
            fragmentEditor.Document.IsReadOnly = !fragmentEditorActive;
            footerEditor.Document.IsReadOnly = fragmentEditorActive;

            if (fragmentEditorActive)
            {
                fragmentEditor.Document.SetHeaderAndFooterText(headerEditor.Document.CurrentSnapshot.Text, footerEditor.Document.CurrentSnapshot.Text);
                fragmentEditor.Focus();
            }
            else
                headerEditor.Focus();
        }

        /// <summary>
        /// Occurs when a mouse is double-clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        private void OnErrorListViewDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            IParseError error = listBox.SelectedItem as IParseError;
            if (error != null)
            {
                fragmentEditor.ActiveView.Selection.StartPosition = error.PositionRange.StartPosition;
                fragmentEditor.Focus();
            }
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.DocumentChanged</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="EditorDocumentChangedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorDocumentChanged(object sender, EditorDocumentChangedEventArgs e)
        {
            AppendMessage("DocumentChanged");
        }
        /// <summary>
        /// Occurs when the <c>SyntaxEditor.DocumentIsModifiedChanged</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorDocumentIsModifiedChanged(object sender, RoutedEventArgs e)
        {
            AppendMessage(String.Format("DocumentIsModifiedChanged: IsModified={0}", fragmentEditor.Document.IsModified));
        }

        /// <summary>
        /// Occurs when the document's parse data has changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <c>EventArgs</c> that contains data related to this event.</param>
        private void OnSyntaxEditorDocumentParseDataChanged(object sender, EventArgs e)
        {
            //
            // NOTE: The parse data here is generated in a worker thread... this event handler is called 
            //         back in the UI thread immediately when the worker thread completes... it is best
            //         practice to delay UI updates until the end user stops typing... we will flag that
            //         there is a pending parse data change, which will be handled in the 
            //         UserInterfaceUpdate event
            //

            hasPendingParseData = true;
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.IsOverwriteModeActiveChanged</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="BooleanPropertyChangedRoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorIsOverwriteModeActiveChanged(object sender, BooleanPropertyChangedRoutedEventArgs e)
        {
            AppendMessage("IsOverwriteModeActiveChanged");
            //overwriteModePanel.Content = (e.NewValue ? "OVR" : "INS");
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.MacroRecordingStateChanged</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorMacroRecordingStateChanged(object sender, RoutedEventArgs e)
        {
            AppendMessage("MacroRecordingStateChanged: " + fragmentEditor.MacroRecording.State);

            //switch (editor.MacroRecording.State)
            //{
            //    case MacroRecordingState.Recording:
            //        messagePanel.Content = "Macro recording is active";
            //        recordMacroButtonImage.Source = new BitmapImage(new Uri("/Resources/Images/MacroRecordingStop16.png", UriKind.Relative));
            //        recordMacroButton.ToolTip = "Stop Recording";
            //        pauseRecordingButton.IsChecked = false;
            //        pauseRecordingButton.ToolTip = "Pause Recording";
            //        break;
            //    case MacroRecordingState.Paused:
            //        messagePanel.Content = "Macro recording is paused";
            //        pauseRecordingButton.IsChecked = true;
            //        pauseRecordingButton.ToolTip = "Resume Recording";
            //        break;
            //    default:
            //        messagePanel.Content = "Ready";
            //        recordMacroButtonImage.Source = new BitmapImage(new Uri("/Resources/Images/MacroRecordingRecord16.png", UriKind.Relative));
            //        recordMacroButton.ToolTip = "Record Macro";
            //        pauseRecordingButton.IsChecked = false;
            //        pauseRecordingButton.ToolTip = "Pause Recording";
            //        break;
            //}

            //recordMacroMenuItem.Header = recordMacroButton.ToolTip;
            //pauseRecordingMenuItem.IsChecked = pauseRecordingButton.IsChecked.Value;
            //pauseRecordingMenuItem.Header = pauseRecordingButton.ToolTip;
        }

        /// <summary>
        /// Occurs after a brief delay following any document text, parse data, or view selection update, allowing consumers to update the user interface during an idle period.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains data related to this event.</param>
        private void OnSyntaxEditorUserInterfaceUpdate(object sender, RoutedEventArgs e)
        {
            // If there is a pending parse data change...
            if (hasPendingParseData)
            {
                // Clear flag
                hasPendingParseData = false;

                ILLParseData parseData = fragmentEditor.Document.ParseData as ILLParseData;
                if (parseData != null)
                {
                    if (fragmentEditor.Document.CurrentSnapshot.Length < 10000)
                    {
                        // Show the AST
                        if (parseData.Ast != null)
                            ;
                        //    astOutputEditor.Document.SetText(parseData.Ast.ToTreeString(0));
                        //else
                        //    astOutputEditor.Document.SetText(null);
                    }
                    //else
                    //    astOutputEditor.Document.SetText("(Not displaying large AST for performance reasons)");

                    // Output errors
                    errorListView.ItemsSource = parseData.Errors;
                }
                else
                {
                    // Clear UI
                    //astOutputEditor.Document.SetText("(Language may not have AST building features)");
                    errorListView.ItemsSource = null;
                }
            }
        }

        /// <summary>
        /// Occurs when the incremental search mode of an <see cref="ITextView"/> is activated or deactivated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="EditorViewSelectionEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewIsIncrementalSearchActiveChanged(object sender, TextViewEventArgs e)
        {
            //IEditorView editorView = e.View as IEditorView;
            //if ((editorView != null) && (!editorView.IsIncrementalSearchActive))
            //{
            //    // Incremental search is now deactivated
            //    messagePanel.Content = "Ready";
            //}
        }

        /// <summary>
        /// Occurs when a search operation occurs in a view.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="EditorViewSearchEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewSearch(object sender, EditorViewSearchEventArgs e)
        {
            //// If an incremental search was performed...
            //if (e.ResultSet.OperationType == SearchOperationType.FindNextIncremental)
            //{
            //    // Show a statusbar message
            //    bool hasFindText = !String.IsNullOrEmpty(e.ResultSet.Options.FindText);
            //    bool notFound = (hasFindText) && (e.ResultSet.Results.Count == 0);
            //    string notFoundMessage = (notFound ? " (not found)" : String.Empty);
            //    messagePanel.Content = "Incremental Search: " + e.ResultSet.Options.FindText + notFoundMessage;
            //}
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.ViewSelectionChanged</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="EditorViewSelectionEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewSelectionChanged(object sender, EditorViewSelectionEventArgs e)
        {
            // Quit if this event is not for the active view
            if (!e.View.IsActive)
                return;

            //// Update line, col, and character display
            //linePanel.Text = String.Format("Ln {0}", e.CaretPosition.DisplayLine);
            //columnPanel.Text = String.Format("Col {0}", e.CaretDisplayCharacterColumn);
            //characterPanel.Text = String.Format("Ch {0}", e.CaretPosition.DisplayCharacter);

            //// If token info should be displayed in the statusbar...
            //if (toggleTokenInfoMenuItem.IsChecked)
            //{
            //    // Get a snapshot reader
            //    ITextSnapshotReader reader = e.View.CurrentSnapshot.GetReader(e.View.Selection.EndOffset);
            //    IToken token = reader.Token;
            //    if (token != null)
            //    {
            //        IMergableToken mergableToken = token as IMergableToken;
            //        if (mergableToken != null)
            //            tokenPanel.Content = String.Format("{0} / {1} / {2}{3}",
            //                mergableToken.Lexer.Key, mergableToken.LexicalState.Key,
            //                token.Key, (e.View.Selection.EndOffset == token.StartOffset ? "*" : String.Empty));
            //        else
            //            tokenPanel.Content = String.Format("{0} / {1}{2}", e.View.SyntaxEditor.Document.Language.Key,
            //                token.Key, (e.View.Selection.EndOffset == token.StartOffset ? "*" : String.Empty));
            //        return;
            //    }
            //}
            //tokenPanel.Content = null;
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.ViewSplitAdded</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewSplitAdded(object sender, RoutedEventArgs e)
        {
            AppendMessage("ViewSplitAdded");
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.ViewSplitMoved</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewSplitMoved(object sender, RoutedEventArgs e)
        {
            AppendMessage("ViewSplitMoved");
        }

        /// <summary>
        /// Occurs when the <c>SyntaxEditor.ViewSplitRemoved</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnSyntaxEditorViewSplitRemoved(object sender, RoutedEventArgs e)
        {
            AppendMessage("ViewSplitRemoved");
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies the UI that it has been unloaded.
        /// </summary>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clear .NET Languages Add-on project assembly references when the sample unloads
            projectAssembly.AssemblyReferences.Clear();
        }
    }
}

