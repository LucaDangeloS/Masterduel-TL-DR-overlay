using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.Overlay.Icons
{
    public class IconScheme
    {
        private Dictionary<int, Icon> _iconMap = new();

        public void AddIcon(int key, Icon icon)
        {
            _iconMap.Add(key, icon);
        }
        public void AddIcon(int key, string path, Size resolution)
        {
            _iconMap.Add(key, new Icon(path, resolution));
        }
        public void RemoveIcon(int key)
        {
            _iconMap.Remove(key);
        }
        public Icon GetIcon(int key)
        {
            return _iconMap[key];
        }
        public void ChangeResolution(Size resolution)
        {
            foreach (int key in _iconMap.Keys)
            {
                var i = _iconMap[key];
                _iconMap[key]= new Icon(i.Path, i.Resolution);
            }
        }
        public void ClearIcons()
        {
            _iconMap.Clear();
        }
    }
}
