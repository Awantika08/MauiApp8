using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp8.Entities;
public class Mood
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public MoodCategory Category { get; set; }
}
