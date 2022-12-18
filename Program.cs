using Terminal.Gui;
using GitignoreIo;
using System.Reflection;

namespace mkgig
{
    internal class Program
    {
        private static ListView _listView = CreateListView();
        private static Label _filterLabel = CreateFilterLabel();
        private static string _fileName = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");

        static void Main(string[] args)
        {
            Application.Init();

            var mainWindow = CreateWindow();
            mainWindow.Add(_listView);
            mainWindow.Add(_filterLabel);

            var top = Application.Top;
            top.Add(mainWindow);
            top.Add(CreateStatusBar());
            top.LayoutComplete += (_) =>
            {
                top.Add(CreateFilenameLabel(_fileName, Pos.Left(mainWindow) + 1, Pos.Bottom(mainWindow)));
            };
            Application.Run();
            Application.Shutdown();
        }

        static ListView CreateListView()
        {
            var listView = new ListView
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = true,
                AllowsMultipleSelection = true,
            };

            listView.Initialized += async (object? sender, EventArgs args) =>
            {
                var templates = (await Repository.GetTemplateNames());
                _listView.Source = new TemplateListDataSource(templates);
            };
            return listView;
        }

        static Label CreateFilterLabel()
        {
            return new Label
            {
                AutoSize = true,
                X = 2,
                Y = 0,
                Height = 1,
                ColorScheme = new ColorScheme
                {
                    Normal = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
                },
            };
        }

        static Label CreateFilenameLabel(string text, Pos x, Pos y)
        {
            return new Label
            {
                X = x,
                Y = y,
                Width = Dim.Fill(),
                Height = 1,
                ColorScheme = Colors.TopLevel,
                Text = text,
            };
        }

        static Window CreateWindow()
        {
            var window = new Window("Templates")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2,
            };
            window.KeyPress += (e) => 
            {
                var filterString = _filterLabel.Text.ToString() ?? string.Empty;
                var key = e.KeyEvent.Key;
                if (key == Key.Esc)
                {
                    e.Handled = true;
                    filterString = string.Empty;
                }
                else if (key > Key.Space && key < Key.Delete)
                {
                    e.Handled = true;
                    filterString = _filterLabel.Text.ToString() + (char)key;
                } 
                else if (key == Key.Backspace && filterString.Length > 0)
                {
                    e.Handled = true;
                    filterString = string.Concat(filterString.Take(filterString.Length - 1));
                }
                if (e.Handled)
                {
                    _filterLabel.Text = filterString;
                    (_listView.Source as TemplateListDataSource)?.SetFilter(filterString);
                    _listView.MoveHome(); // In case the selected item is no longer visible
                }
            };
            return window;
        }

        static StatusBar CreateStatusBar()
        {
            return new StatusBar(new[] {
                    new StatusItem(Key.F1, "~F1~ Help", Help),
                    new StatusItem(Key.CtrlMask | Key.A, "~^A~ About", About),
                    new StatusItem(Key.CtrlMask | Key.S, "~^S~ Save", Save),
                    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", Quit),
                });
        }

        enum MessageBoxOption { None = -1, Overwrite, Append, Cancel };
        private static async void Save()
        {
            var selectedTemplates = (_listView.Source as TemplateListDataSource)?.MarkedTemplateNames;
            if (selectedTemplates == null || !selectedTemplates.Any())
            {
                MessageBox.ErrorQuery(50, 7, "Nothing Selected", "\nSelect one or more templates and try again.", "OK");
            }
            else
            {
                if (File.Exists(_fileName))
                {
                    var prompt = $"\n{_fileName} exists.\n\nDo you want to overwrite it or append to it?";
                    var option = (MessageBoxOption)MessageBox.ErrorQuery(50, 9, "Overwrite", prompt, MessageBoxOption.Overwrite.ToString(), MessageBoxOption.Append.ToString(), MessageBoxOption.Cancel.ToString());
                    if (option == MessageBoxOption.Overwrite || option == MessageBoxOption.Append)
                    {
                        await SaveGitignore(_fileName, selectedTemplates, option == MessageBoxOption.Append);
                        Quit();
                    }
                }
                else
                {
                    await SaveGitignore(_fileName, selectedTemplates);
                    Quit();
                }
            }
            static async Task SaveGitignore(string filePath, IEnumerable<string> selectedTemplates, bool append = false)
            {
                try
                {
                    var contents = await Repository.GetTemplate(selectedTemplates.ToArray());
                    using var writer = new StreamWriter(filePath, append);
                    await writer.WriteAsync(contents);
                }
                catch (IOException ex)
                {
                    MessageBox.ErrorQuery("Error", ex.Message);
                }
            }
        }

        private static void Quit()
        {
            Application.RequestStop();
        }

        private static void Help()
        {
            const string helpText = @"
Use this app to create a .gitignore file for one or more
operating systems, programming languages or IDEs, using 
templates from https://www.toptal.com/developers/gitignore/.

* Select the templates to include in the file.
  - Use the up and down arrows to highlight a template.
  - Press the spacebar to select the highlighted template.
  - Type all or part of a template's name to filter the list.
* Press Ctrl+S to write the .gitignore file to disk. 
  - The path and file name are shown below the list.
  - If the .gitignore file aleady exists, you will be given 
    the option to overwrite it or append to it.
* Press Ctrl+Q to close the app without writing the .gitignore 
  file.
";
            var dialog = new Dialog {
                Title = "Using mkgig",
                Width = 68,
                Height = 20,
            };
            var label = new Label(helpText){
				X = 1,
				Y = 0,
				Width = Dim.Fill() - 2,
				Height = Dim.Fill(),
			};
            dialog.Add(label);

            var button = new Button("OK"){
				X = Pos.Center(),
				Y = Pos.Bottom(label) - 2,
				IsDefault = true,
			};
            button.Clicked += () => Application.RequestStop(); 
            dialog.Add(button);

            Application.Run(dialog);
        }

        private static void About()
        {
            const string logo = @"
                  █████                ███          
                 ░░███                ░░░           
  █████████████   ░███ █████  ███████ ████   ███████
 ░░███░░███░░███  ░███░░███  ███░░███░░███  ███░░███
  ░███ ░███ ░███  ░██████░  ░███ ░███ ░███ ░███ ░███
  ░███ ░███ ░███  ░███░░███ ░███ ░███ ░███ ░███ ░███
  █████░███ █████ ████ █████░░███████ █████░░███████
 ░░░░░ ░░░ ░░░░░ ░░░░ ░░░░░  ░░░░░███░░░░░  ░░░░░███
                             ███ ░███       ███ ░███
                            ░░██████       ░░██████ 
                             ░░░░░░         ░░░░░░    ";
            var acknowlegment = "API and templates provided by https://www.toptal.com/developers/gitignore/";
            var assembly = Assembly.GetEntryAssembly();
            var version = (assembly == null) ? string.Empty : assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            var copyright = (assembly == null) ? string.Empty : assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            var description = (assembly == null) ? string.Empty : assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?
                .Description;
            MessageBox.Query("About mkgig", $"{logo}\n{description}\nVersion {version}\n{copyright}\n\n{acknowlegment}\n\n", "Ok");
        }
    }
}
