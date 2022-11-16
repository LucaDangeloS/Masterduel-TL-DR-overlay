using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.Overlay.Icons
{
    /// <summary>
    /// This class generalized the IconScheme, internally storing a mapping from ints to Icons and the capability of changing the size
    /// of all the icons simultaneously.
    /// </summary>
    public class IconScheme
    {
        private readonly Dictionary<int, Icon> _iconMap = new();

        /// <summary>
        /// Add an icon to the IconScheme directly passing the <see cref="Icon"/> object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="size"></param>
        public void AddIcon(int key, Icon icon)
        {
            _iconMap.Add(key, icon);
        }
        /// <summary>
        /// Add an icon to the IconScheme with the path of the image file and the size of the icon.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="size"></param>
        public void AddIcon(int key, string path, Size size)
        {
            _iconMap.Add(key, new Icon(path, size));
        }

        
        public void RemoveIcon(int key)
        {
            _iconMap.Remove(key);
        }
        public Icon GetIcon(int key)
        {
            return _iconMap[key];
        }

        /// <summary>
        /// Changes the size of all the icons within the IconScheme.
        /// </summary>
        /// <param name="size"></param>
        public void ChangeResolution(Size size)
        {
            foreach (int key in _iconMap.Keys)
            {
                var i = _iconMap[key];
                _iconMap[key] = new Icon(i.Path, size);
            }
        }
        
        public void ClearIcons()
        {
            _iconMap.Clear();
        }
    }
}
