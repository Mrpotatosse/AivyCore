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
        static readonly VisualizerManager manager = new VisualizerManager();

        public static void OpenUI()
        {
            manager.SafeRun(() =>
            {
                manager.ShowUI(true);
            });
        }

        public static void HideUI()
        {
            manager.SafeRun(() =>
            {
                manager.ShowUI(false);
            });
        }

        public static DofusMapVisualizer Visualizer
        {
            get
            {
                DofusMapVisualizer result = null;

                manager.SafeRun(() =>
                {
                    result = manager.window;
                });

                return result;
            }
        }

        readonly DofusMapVisualizer window;// = new DofusMapVisualizer();

        private VisualizerManager()
        {
            window = new DofusMapVisualizer();
        }

        public async void ShowUI(bool show)
        {
            if (show)
                await Task.Run(window.ShowDialog);
            else
                await Task.Run(window.Hide);
        }
    }
}
