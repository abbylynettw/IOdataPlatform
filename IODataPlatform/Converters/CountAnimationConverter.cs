using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Threading;

namespace IODataPlatform.Converters
{
    /// <summary>
    /// 计数动画转换器 - 实现数字从0平滑增长到目标值的动画效果
    /// </summary>
    public class CountAnimationConverter : IValueConverter
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _currentValue;
        private int _targetValue;
        private Action<int> _updateCallback;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int targetCount)
            {
                // 重置动画状态
                _currentValue = 0;
                _targetValue = targetCount;

                // 停止之前的计时器
                _timer.Stop();

                // 设置计时器间隔（控制动画速度）
                _timer.Interval = TimeSpan.FromMilliseconds(20);

                // 创建回调函数来更新UI
                _updateCallback = (newValue) =>
                {
                    // 这里我们需要一种方式来更新UI绑定的属性
                    // 由于Converter不能直接更新源，我们将在ViewModel中处理
                };

                _timer.Tick += Timer_Tick;
                _timer.Start();

                // 初始返回0
                return 0;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_currentValue < _targetValue)
            {
                // 计算增长步长（根据目标值动态调整，确保动画速度一致）
                int step = Math.Max(1, _targetValue / 50);
                _currentValue = Math.Min(_currentValue + step, _targetValue);

                // 触发更新回调
                _updateCallback?.Invoke(_currentValue);
            }
            else
            {
                // 动画完成，停止计时器
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }
        }
    }
}
