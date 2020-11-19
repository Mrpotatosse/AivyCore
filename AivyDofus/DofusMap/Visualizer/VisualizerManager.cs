using AivyDofus.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AivyDofus.DofusMap.Visualizer
{
    public class VisualizerManager : SafeSingelton
    {
        public class VisualizerManagerInstance : VisualizerManager
        {
            public event Func<int, DofusMapVisualizer> VisualizerGetter;

            public DofusMapVisualizer GetVisualizer(int port)
            {
                if (VisualizerGetter is null) return null;
                return VisualizerGetter(port);
            }
        }

        static readonly VisualizerManager manager = new VisualizerManager();

        public static void OpenUI(int port)
        {
            manager.SafeRun(() =>
            {
                manager.ShowUI(port);
            });
        }

        // for lua
        public static VisualizerManagerInstance LuaInstance
        {
            get
            {
                VisualizerManagerInstance instance = new VisualizerManagerInstance();
                instance.VisualizerGetter += Visualizer;

                return instance;
            }
        }

        public static DofusMapVisualizer Visualizer(int port)
        {
            DofusMapVisualizer result = null;

            manager.SafeRun(() =>
            {
                result = manager[port];
            });

            return result;
        }

        readonly Dictionary<int, DofusMapVisualizer> windows;// = new DofusMapVisualizer();

        private VisualizerManager()
        {
            windows = new Dictionary<int, DofusMapVisualizer>();
        }

        public DofusMapVisualizer this[int port]
        {
            get
            {
                if (!windows.ContainsKey(port))
                    windows.Add(port, new DofusMapVisualizer());
                return windows[port];                
            }
        }

        public async void ShowUI(int port)
        {
            if (!windows.ContainsKey(port))
            {
                windows.Add(port, new DofusMapVisualizer());
            }
            DofusMapVisualizer window = windows[port];
            await Task.Run(window.ShowDialog);
        }
    }
}
