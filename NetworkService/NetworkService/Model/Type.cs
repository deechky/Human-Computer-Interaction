using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Type
    {
        string name;
        string img_src;

        public string Name { get => name; set => name = value; }
        public string Img_src { get => img_src; set => img_src = value; }

        public Type(string type)
        {
            Name = type;
            // Ažurirane putanje slika za T1 specifikaciju
            if (type == "Kablovski senzor")
            {
                Img_src = "/Resources/Images/cable_sensor.jpg";
            }
            else if (type == "Digitalni manometar")
            {
                Img_src = "/Resources/Images/digital_manometer.jpg";
            }
            else
            {
                // Putanja za nepoznati tip
                Img_src = "/Resources/Images/unknown.jpg";
            }
        }

        public override bool Equals(object obj)
        {
            string s = obj as string;
            return s == Name;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}