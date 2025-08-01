using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementViewer.Data;
public class Achievement_Rank
{
    public int? Server { get; set; }
    public int? Data_Center { get; set; }
    public int? Global { get; set; }
    public Achievement_Rank() { }
}
