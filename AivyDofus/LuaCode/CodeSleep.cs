using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AivyDofus.LuaCode
{
    public class CodeSleep
    {
        public CodeSleep()
        {

        }

        private CancellationTokenSource _source { get; set; }

        public async void sleep_and_continue(double value, Action on_end)
        {
            if (on_end is null) throw new ArgumentNullException(nameof(on_end));

            try
            {
                _source = new CancellationTokenSource();

                await Task.Delay(TimeSpan.FromMilliseconds(value), _source.Token).ContinueWith(task =>
                {
                    try
                    {
                        on_end();
                    }
                    catch (Exception e)
                    {
                        _source.Cancel();
                    }
                }, _source.Token);
            }
            catch(Exception e)
            {
                _source.Cancel();
            }
        }
    }
}
