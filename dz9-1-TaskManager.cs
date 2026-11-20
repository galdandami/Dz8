using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagerApp
{
    public enum ProjectStatus
    {
        Проект,
        Исполнение,
        Закрыт
    }

    public enum TaskStatus
    {
        Назначена,
        ВРаботе,
        НаПроверке,
        Выполнена
    }


    public class Report //отчет
    {
        public string Text;
        public DateTime CompletionDate;
        public string ExecutorName;

        public Report(string text, string executorName)
        {
            Text = text;
            CompletionDate = DateTime.Now;
            ExecutorName = executorName;
        }
    }


    public class ProjectTask // задачи
    {
        public int Id;
        public string Description; //описание
        public DateTime Deadline;
        public string Initiator;
        public string Executor; // Может быть null
        public TaskStatus Status;
        public Report TaskReport;
        
        // Для периодических задач (доп. задание)
        public bool IsPeriodic;
        public int PeriodInDays;
        public List<Report> ReportsHistory = new List<Report>();

        private static int _idCounter = 1;

        public ProjectTask(string description, DateTime deadline, string initiator)
        {
            Id = _idCounter++;
            Description = description;
            Deadline = deadline;
            Initiator = initiator;
            Executor = null; // Изначально не назначен
            Status = TaskStatus.Назначена; 
        }

        public void CreateReport(string text)
        {
            if (Status != TaskStatus.ВРаботе)
            {
                Console.WriteLine("Отчет можно сдать только из статуса В работе");
                return;
            }

            var report = new Report(text, Executor);
            TaskReport = report;
            ReportsHistory.Add(report);
            Status = TaskStatus.НаПроверке;
            Console.WriteLine($"Задача {Id}. Отчет сдан, статус изменен на 'На проверке'.");
        }
    }


    public class Project
    {

        public string Description;
        public DateTime StartDate;
        public DateTime EndDate;
        public string Initiator;
        public string TeamLead;
        public List<ProjectTask> Tasks = new List<ProjectTask>();
        public ProjectStatus Status;

        public Project(string description, string initiator, string teamLead)
        {
            Description = description;
            Initiator = initiator;
            TeamLead = teamLead;
            StartDate = DateTime.Now;
            Status = ProjectStatus.Проект;
        }

        // Создание задачи
        public void AddTask(string desc, DateTime deadline, string initiator)
        {
            if (Status != ProjectStatus.Проект)
            {
                Console.WriteLine("Нельзя добавлять задачи, проект уже в исполнении или закрыт.");
                return;
            }
            Tasks.Add(new ProjectTask(desc, deadline, initiator));
        }

        // Перевод проекта в исполнение
        public void StartProject()
        {
            if (Status == ProjectStatus.Проект)
            {
                Status = ProjectStatus.Исполнение;
                Console.WriteLine("Проект переведен в статус Исполнение");
            }
        }
    }


    public class TaskManager
    {
        private List<Project> _projects = new List<Project>();

        public Project CreateProject(string desc, string initiator, string teamLead)
        {
            var p = new Project(desc, initiator, teamLead);
            _projects.Add(p);
            Console.WriteLine($"Создан проект: {desc}");
            return p;
        }

        public void PrintAllTasks(Project project)
        {
            Console.WriteLine($"\n Задачи проекта: {project.Description}");
            foreach (var t in project.Tasks)
            {
                Console.WriteLine($"ID: {t.Id}, {t.Description}, Статус: {t.Status}, Исполнитель: {t.Executor}");
            }
        }


        public void TakeTask(Project project, int taskId, string executor)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) { Console.WriteLine("Задача не найдена."); return; }

            if (task.Status == TaskStatus.Назначена && task.Executor == null)
            {
                task.Executor = executor;
                task.Status = TaskStatus.ВРаботе;
                Console.WriteLine($"Сотрудник {executor} взял задачу #{taskId} в работу.");
            }
            else if (task.Status == TaskStatus.Назначена && task.Executor != null)
            {
                 Console.WriteLine($"Задача уже назначена на {task.Executor}. Нельзя взять дважды.");
            }
        }

        // Делегировать задачу
        public void DelegateTask(Project project, int taskId, string newExecutor)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return;

            if (task.Status == TaskStatus.ВРаботе || task.Status == TaskStatus.Назначена)
            {
                task.Executor = newExecutor;
                Console.WriteLine($"Задача #{taskId} делегирована на {newExecutor}. Статус: 'Назначена'.");
            }
            else
            {
                Console.WriteLine("Нельзя делегировать задачу в текущем статусе.");
            }
        }

        public void ApproveReport(Project project, int taskId)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null && task.Status == TaskStatus.НаПроверке)
            {
                task.Status = TaskStatus.Выполнена;
                Console.WriteLine($"Отчет по задаче #{taskId} утвержден. Задача выполнена.");
                CheckProjectClosure(project);
            }
        }

        public void RejectReport(Project project, int taskId)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null && task.Status == TaskStatus.НаПроверке)
            {
                task.Status = TaskStatus.ВРаботе;
                Console.WriteLine($"Отчет по задаче #{taskId} отклонен. Задача возвращена в работу.");
                task.Status = TaskStatus.ВРаботе;
            }
        }

        // Проверка, можно ли закрыть проект
        public void CheckProjectClosure(Project project)
        {
            if (project.Tasks.All(t => t.Status == TaskStatus.Выполнена))
            {
                project.Status = ProjectStatus.Закрыт;
                Console.WriteLine("Все задачи выполнены. Проект закрыт");
            }
            else
            {
                Console.WriteLine("Выполнены не все задачи");
            }
        }
        
    }

    class Program
    {
        static void Main()
        {
            TaskManager manager = new TaskManager();
            
            manager.CreateProject("Разработка CRM", "Директор", "Иванов");
            Project crmProject = new Project("Разработка CRM", "Директор", "Иванов");
            
            // Создание задач
            crmProject.AddTask("Сверстать главную", DateTime.Now.AddDays(5), "Директор");
            crmProject.AddTask("Написать бэкенд", DateTime.Now.AddDays(10), "Директор");
            
            // Старт проекта
            crmProject.StartProject();

            // Вывод задач
            manager.PrintAllTasks(crmProject);

            // Взятие задачи в работу
            manager.TakeTask(crmProject, 1, "Петров"); // ID=1 (Сверстать)
            manager.TakeTask(crmProject, 2, "Сидоров"); // ID=2 (Бэкенд)

            // Сдача отчета 
            var task1 = crmProject.Tasks[0];
            task1.CreateReport("Главная страница готова");

            // Утверждение отчета
            manager.ApproveReport(crmProject, 1);

            // Попытка закрыть проект (не все задачи выполнены)
            manager.CheckProjectClosure(crmProject);
        }
    }
}
