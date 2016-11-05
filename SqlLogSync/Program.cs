using SqlLogSync.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var DBTask = DatabaseTask.Create();

            DBTask.Run();
        }
    }
}
