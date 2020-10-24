using NLog;
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
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CodeSleep()
        {

        }

        private CancellationTokenSource _source { get; set; }

        public void sleep_and_continue(double value, Action on_end)
        {
            if (on_end is null) throw new ArgumentNullException(nameof(on_end));

            try
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(value));
                on_end();
            }
            catch(Exception e)
            {
                logger.Error(e);
            }        
        }

        public async void async_sleep_and_continue(double value, Action on_end)
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
                        logger.Error(e);
                        _source.Cancel();

                    }
                }, _source.Token);
            }
            catch(Exception e)
            {
                logger.Error(e);
                _source.Cancel();
            }
        }
    }
}
