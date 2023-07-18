using System.Collections;
using Terminal.Gui;

namespace mkgig
{
    /* 
    This class provides a list of template names that can be 
    filtered from the display without being removed from the store.
    */
    internal class TemplateListDataSource : IListDataSource
    {
        private class GitignoreTemplate
        {
            public bool IsMarked { get; set; }
            public bool IsVisible { get; set; }
            public string TemplateName { get; init; } = string.Empty;
        }

        private List<GitignoreTemplate> Templates { get; set; }

        public int Count => VisibleTemplates.Count();

        public int Length => VisibleTemplates.Max(t => t.TemplateName.Length);

        public bool IsMarked(int item) => VisibleTemplates.ToArray()[item].IsMarked;

        public IEnumerable<string> MarkedTemplateNames => Templates
            .Where(template => template.IsMarked)
            .Select(template => template.TemplateName);

        private IEnumerable<GitignoreTemplate> VisibleTemplates => Templates.Where(t => t.IsVisible);

        public TemplateListDataSource(IEnumerable<string> templateNames)
        {
            Templates = templateNames.Select(templateName =>
                new GitignoreTemplate
                {
                    IsMarked = false,
                    IsVisible = true,
                    TemplateName = templateName,
                }).ToList();
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line,
            int width, int start = 0)
        {
            container.Move(col, line);
            driver.AddStr(VisibleTemplates.ToArray()[item].TemplateName.PadRight(width));
        }

        public void SetMark(int item, bool value)
        {
            VisibleTemplates.ToArray()[item].IsMarked = value;
        }

        public IList ToList() => VisibleTemplates.ToList();

        internal void SetFilter(string filterString)
        {
            foreach (var template in Templates)
            {
                template.IsVisible = template.TemplateName.Contains(filterString);
            }
        }
    }
}