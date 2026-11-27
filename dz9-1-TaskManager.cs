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


    public class Report
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


    public class ProjectTask
    {
        public int Id;
        public string Description;
        public DateTime Deadline;
        public string Initiator;
        public string Executor;
        public TaskStatus Status;
        public Report TaskReport;
        
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
            Executor = null;
            Status = TaskStatus.Назначена; 
        }

        public void CreateReport(string text, string user)
        {
            if (user != Executor)
            {
                Console.WriteLine("Отчет может сдать только исполнитель задачи");
                return;
            }

            if (Status != TaskStatus.ВРаботе)
            {
                Console.WriteLine("Отчет можно сдать только из статуса В работе");
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Текст отчета не может быть пустым");
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
        public List<string> Team = new List<string>();
        public ProjectStatus Status;

        public const int MinTeamSize = 10;

        public Project(string description, string initiator, string teamLead)
        {
            Description = description;
            Initiator = initiator;
            TeamLead = teamLead;
            StartDate = DateTime.Now;
            Status = ProjectStatus.Проект;
        }

        public void AddTeamMember(string user, string member)
        {
            if (user != TeamLead)
            {
                Console.WriteLine("Состав команды может менять только тимлид.");
                return;
            }

            if (Status != ProjectStatus.Проект)
            {
                Console.WriteLine("Нельзя менять команду, проект уже в исполнении или закрыт.");
                return;
            }

            if (Team.Contains(member))
            {
                Console.WriteLine($"Сотрудник {member} уже в команде.");
                return;
            }

            Team.Add(member);
        }

        public void AddTask(string desc, DateTime deadline, string initiator)
        {
            if (initiator != TeamLead)
            {
                Console.WriteLine("Задачи может создавать только тимлид проекта.");
                return;
            }

            if (Status != ProjectStatus.Проект)
            {
                Console.WriteLine("Нельзя добавлять задачи, проект уже в исполнении или закрыт.");
                return;
            }
            Tasks.Add(new ProjectTask(desc, deadline, initiator));
        }

        public void StartProject(string user)
        {
            if (user != TeamLead)
            {
                Console.WriteLine("Перевести проект в исполнение может только тимлид.");
                return;
            }

            if (Team.Count < MinTeamSize)
            {
                Console.WriteLine($"В команде должно быть не менее {MinTeamSize} человек, сейчас: {Team.Count}. Проект не переведен в исполнение.");
                return;
            }

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

            if (!project.Team.Contains(executor))
            {
                Console.WriteLine($"Сотрудник {executor} не входит в команду проекта.");
                return;
            }

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

        public void DelegateTask(Project project, int taskId, string user, string newExecutor)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return;

            if (user != task.Executor)
            {
                Console.WriteLine("Делегировать задачу может только ее исполнитель.");
                return;
            }

            if (!project.Team.Contains(newExecutor))
            {
                Console.WriteLine($"Сотрудник {newExecutor} не входит в команду проекта.");
                return;
            }

            if (task.Status == TaskStatus.ВРаботе || task.Status == TaskStatus.Назначена)
            {
                task.Executor = newExecutor;
                task.Status = TaskStatus.Назначена;
                Console.WriteLine($"Задача #{taskId} делегирована на {newExecutor}. Статус: 'Назначена'.");
            }
            else
            {
                Console.WriteLine("Нельзя делегировать задачу в текущем статусе.");
            }
        }

        public void ApproveReport(Project project, int taskId, string user)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) { Console.WriteLine("Задача не найдена."); return; }

            if (user != task.Initiator)
            {
                Console.WriteLine("Утвердить отчет может только инициатор задачи.");
                return;
            }

            if (task.Status != TaskStatus.НаПроверке || task.TaskReport == null)
            {
                Console.WriteLine("Нет отчета на проверке.");
                return;
            }

            task.Status = TaskStatus.Выполнена;
            Console.WriteLine($"Отчет по задаче #{taskId} утвержден. Задача выполнена.");
            CheckProjectClosure(project);
        }

        public void RejectReport(Project project, int taskId, string user)
        {
            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) { Console.WriteLine("Задача не найдена."); return; }

            if (user != task.Initiator)
            {
                Console.WriteLine("Вернуть отчет на доработку может только инициатор задачи.");
                return;
            }

            if (task.Status != TaskStatus.НаПроверке || task.TaskReport == null)
            {
                Console.WriteLine("Нет отчета на проверке.");
                return;
            }

            task.ReportsHistory.Remove(task.TaskReport);
            task.TaskReport = null;
            task.Status = TaskStatus.ВРаботе;
            Console.WriteLine($"Отчет по задаче #{taskId} отклонен. Задача возвращена в работу.");
        }

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

            string[] members = { "Петров", "Сидоров", "Кузнецов", "Смирнов", "Попов", "Васильев", "Соколов", "Михайлов", "Новиков", "Федоров" };
            foreach (var m in members)
            {
                crmProject.AddTeamMember("Иванов", m);
            }
            
            crmProject.AddTask("Сверстать главную", DateTime.Now.AddDays(5), "Иванов");
            crmProject.AddTask("Написать бэкенд", DateTime.Now.AddDays(10), "Иванов");
            
            crmProject.StartProject("Иванов");

            manager.PrintAllTasks(crmProject);

            manager.TakeTask(crmProject, 1, "Петров");
            manager.TakeTask(crmProject, 2, "Сидоров");

            var task1 = crmProject.Tasks[0];
            task1.CreateReport("Главная страница готова", "Петров");

            manager.RejectReport(crmProject, 1, "Иванов");
            task1.CreateReport("Главная страница доработана", "Петров");

            manager.ApproveReport(crmProject, 1, "Иванов");

            manager.CheckProjectClosure(crmProject);
        }
    }
}