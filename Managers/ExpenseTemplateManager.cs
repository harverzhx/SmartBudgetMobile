using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetTracker.Models;

namespace SmartBudgetMobile.Managers
{
    public class ExpenseTemplateManager
    {
        private List<ExpenseTemplate> _templates;
        private int _nextId;

        public ExpenseTemplateManager()
        {
            _templates = FileManager.LoadTemplates();
            _nextId = _templates.Any() ? _templates.Max(t => t.Id) + 1 : 1;
        }

        public List<ExpenseTemplate> GetTemplatesByUser(string username)
        {
            return _templates.Where(t => t.Username == username)
                             .OrderBy(t => t.TemplateName)
                             .ToList();
        }

        public (bool Success, string Message) AddTemplate(ExpenseTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.TemplateName))
                return (false, "Template name cannot be empty.");

            template.Id = _nextId++;
            template.CreatedAt = DateTime.Now;
            _templates.Add(template);

            Save();
            return (true, "Template added successfully.");
        }

        public (bool Success, string Message) DeleteTemplate(int templateId)
        {
            var template = _templates.FirstOrDefault(t => t.Id == templateId);
            if (template == null)
                return (false, "Template not found.");

            _templates.Remove(template);
            Save();
            return (true, "Template deleted successfully.");
        }

        public void Save()
        {
            FileManager.SaveTemplates(_templates);
        }

        public void Reload()
        {
            _templates = FileManager.LoadTemplates();
            _nextId = _templates.Any() ? _templates.Max(t => t.Id) + 1 : 1;
        }
    }
}
