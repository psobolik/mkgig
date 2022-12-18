using System.Collections;
using NStack;
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
            public string TemplateName { get; set; } = string.Empty;
        }

        private List<GitignoreTemplate> Templates { get; set; } = new();

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
        private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
        {
            int byteLen = ustr.Length;
            int used = 0;
            for (int i = start; i < byteLen;)
            {
                (var rune, var size) = Utf8.DecodeRune(ustr, i, i - byteLen);
                used += Rune.ColumnWidth(rune);
                if (used > width)
                    break;
                driver.AddRune(rune);
                i += size;
            }
            for (; used < width; used++)
            {
                driver.AddRune(' ');
            }
        }

        // private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
        // {
        //     int byteLen = ustr.Length;
        //     int used = 0;
        //     for (int i = start; i < byteLen;)
        //     {
        //         (var rune, var size) = Utf8.DecodeRune(ustr, i, i - byteLen);
        //         var count = Rune.ColumnWidth(rune);
        //         if (used + count > width)
        //             break;
        //         driver.AddRune(rune);
        //         used += count;
        //         i += size;
        //     }
        //     for (; used < width; used++)
        //     {
        //         driver.AddRune(' ');
        //     }
        // }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
        {
            container.Move(col, line);
            var t = VisibleTemplates.ToArray()[item];
            ustring u = t == null ? ustring.Empty : t.TemplateName;
            RenderUstr(driver, u, col, line, width, start);
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