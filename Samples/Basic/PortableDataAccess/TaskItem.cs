using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortableDataAccess
{
    public class TaskItem
    {
        public TaskItem(string name = null, bool complete = false)
        {
            Id = 0;
            Name = name;
            IsComplete = complete;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsComplete { get; set; }
    }
}
