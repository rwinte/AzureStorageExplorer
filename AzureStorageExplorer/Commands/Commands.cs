using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Neudesic.AzureStorageExplorer;

namespace Neudesic.AzureStorageExplorer.Windows
{
    public class Commands
    {
        #region Custom Commands

         /// <summary>
        /// Command for Help / About.
        /// </summary>
        public static RoutedCommand HelpAbout =
          new RoutedCommand("HelpAbout", typeof(MainWindow));

        /// <summary>
        /// Command for Help / Welcome.
        /// </summary>
        public static RoutedCommand HelpWelcome =
          new RoutedCommand("HelpWelcome", typeof(MainWindow));

        /// <summary>
        /// Command for Help / Give Feedback.
        /// </summary>
        public static RoutedCommand HelpFeedback =
          new RoutedCommand("HelpFeedback", typeof(MainWindow));

        /// <summary>
        /// Command for View Errors.
        /// </summary>
        public static RoutedCommand ViewErrors =
          new RoutedCommand("ViewErrors", typeof(MainWindow));

        /// <summary>
        /// Command for Tools / Options.
        /// </summary>
        public static RoutedCommand ToolsOptions =
          new RoutedCommand("ToolsOptions", typeof(MainWindow));

        /// <summary>
        /// Command for Tools / Options.
        /// </summary>
        public static RoutedCommand ToolsCheckForNewVersion =
          new RoutedCommand("ToolsCheckForNewVersion", typeof(MainWindow));

        /// <summary>
        /// Command for Importing of a file.
        /// </summary>
        public static RoutedCommand ImportFile =
          new RoutedCommand("ImportFile", typeof(MainWindow));

        /// <summary>
        /// Command for Exporting of a file.
        /// </summary>
        public static RoutedCommand ExportFile =
          new RoutedCommand("ExportFile", typeof(MainWindow),
                            new InputGestureCollection(new InputGesture[] {
                          new KeyGesture(Key.X, ModifierKeys.Control) }));

        /// <summary>
        /// Command for Changing the View.
        /// </summary>
        public static RoutedCommand ChangeView =
          new RoutedCommand("ChangeView", typeof(MainWindow));
        #endregion

    }
}
