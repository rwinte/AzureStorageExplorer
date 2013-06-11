
// WPF Spinner control from SharpFellows blog post / shared code.
// http://sharpfellows.com/post/WPF-Spinner-e28093-take-two.aspx

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SharpFellows.Toolkit.Controls
{
    public class Marquee : ContentControl
    {
        bool _isLoaded;
        readonly Storyboard _storyboard = new Storyboard();
        readonly DoubleAnimationUsingKeyFrames _animation = new DoubleAnimationUsingKeyFrames();
        FrameworkElement _contentPart;

        public Marquee()
        {
            DefaultStyleKey = typeof(Marquee);
            Loaded += Marquee_Loaded;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentPart = GetTemplateChild("PART_Content") as FrameworkElement;
            SizeChanged += Marquee_SizeChanged;
            if (_contentPart != null) _contentPart.SizeChanged += Marquee_SizeChanged;
        }

        void Marquee_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RestartAnimation();
        }

        void Marquee_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            RestartAnimation();
            MouseEnter += Marquee_MouseEnter;
            MouseLeave += Marquee_MouseLeave;
        }

        void Marquee_MouseLeave(object sender, MouseEventArgs e)
        {
            _storyboard.Resume();
        }

        void Marquee_MouseEnter(object sender, MouseEventArgs e)
        {
            _storyboard.Pause();
        }

        /// <summary>
        /// Length in seconds of the animation effect
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(Marquee), new PropertyMetadata(ValueChanged));

        static void ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Marquee)sender).RestartAnimation();
        }

        /// <summary>
        /// Length in seconds of the pause at the start of the animation
        /// </summary>
        public double InitialPause
        {
            get { return (double)GetValue(InitialPauseProperty); }
            set { SetValue(InitialPauseProperty, value); }
        }

        public static readonly DependencyProperty InitialPauseProperty =
            DependencyProperty.Register("InitialPause", typeof(double), typeof(Marquee), new PropertyMetadata(0d));

        /// <summary>
        /// Length in seconds of the pause at the end of the animation
        /// </summary>
        public double FinalPause
        {
            get { return (double)GetValue(FinalPauseProperty); }
            set { SetValue(FinalPauseProperty, value); }
        }

        public static readonly DependencyProperty FinalPauseProperty =
            DependencyProperty.Register("FinalPause", typeof(double), typeof(Marquee), new PropertyMetadata(0d));

        /// <summary>
        /// Value in the range 0 to 1 representing how much acceleration to apply to the animation. Zero gives a linear animation and 1 is just silly.
        /// </summary>
        public double Acceleration
        {
            get { return (double)GetValue(AccelerationProperty); }
            set { SetValue(AccelerationProperty, value); }
        }

        public static readonly DependencyProperty AccelerationProperty =
            DependencyProperty.Register("Acceleration", typeof(double), typeof(Marquee), new PropertyMetadata(0d));

        private void RestartAnimation()
        {
            if (ActualHeight == 0)
            {
                if (_storyboard != null) _storyboard.Stop();
                return;
            }

            if (_contentPart != null && _isLoaded)
            {
                _storyboard.Stop();

                double value = InitialPause + Duration * _contentPart.ActualWidth / ActualWidth;
                _animation.Duration = new Duration(TimeSpan.FromSeconds(value));

                _animation.KeyFrames.Clear();
                _animation.KeyFrames.Add(new DiscreteDoubleKeyFrame()
                {
                    Value = 0,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))
                });

                _animation.KeyFrames.Add(new DiscreteDoubleKeyFrame()
                {
                    Value = 0,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(InitialPause))
                });

                _animation.KeyFrames.Add(new SplineDoubleKeyFrame()
                {
                    Value = Math.Min(ActualWidth - _contentPart.ActualWidth - 12, 0),
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(value)),
                    KeySpline = new KeySpline()
                    {
                        ControlPoint1 = new Point(Acceleration, 0),
                        ControlPoint2 = new Point(1 - Acceleration, 1)
                    }
                });
                _storyboard.Duration = new Duration(TimeSpan.FromSeconds(value + FinalPause));

                if (_storyboard.Children.Count == 0)
                {
                    _storyboard.Children.Add(_animation);
                    Storyboard.SetTargetProperty(_animation, new PropertyPath("(Canvas.Left)"));
                    Storyboard.SetTarget(_animation, _contentPart);

                    _storyboard.RepeatBehavior = RepeatBehavior.Forever;
                }

                _storyboard.Begin();
            }
        }

    }
}
