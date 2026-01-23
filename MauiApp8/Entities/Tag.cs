using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp8.Entities;
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsPrebuilt { get; set; }
    public List<EntryTag> EntryTags { get; set; } = new();
}
