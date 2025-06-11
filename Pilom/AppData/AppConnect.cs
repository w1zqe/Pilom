using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pilom.AppData
{
    internal class AppConnect
    {
        public static PilomEntities model0db; // Замените YourDbContext на ваш класс контекста
        public static Users CurrentUser { get; set; }
    }
}
