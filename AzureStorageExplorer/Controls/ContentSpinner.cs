
// WPF Spinner control from SharpFellows blog post / shared code.
// http://sharpfellows.com/post/WPF-Spinner-e28093-take-two.aspx

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SharpFellows.Toolkit.Controls
{
    /// <summary>
    /// Simple control providing content spinning capability
    /// </summary>
    public class ContentSpinner : ContentControl
    {
        private const string ANIMATION = "AnimatedRotateTransform";

        #region Fields
        private FrameworkElement _content;
        private Storyboard _storyboard;
        #endregion

        #region Dependency properties

        public static DependencyProperty NumberOfFramesProperty =
            DependencyProperty.Register("NumberOfFrames",
                                        typeof(int),
                                        typeof(ContentSpinner),
                                        new FrameworkPropertyMetadata(16, OnPropertyChange),
                                        ValidateNumberOfFrames);

        public static DependencyProperty RevolutionsPerSecondProperty =
            DependencyProperty.Register("RevolutionsPerSecond",
                                        typeof(double),
                                        typeof(ContentSpinner),
                                        new PropertyMetadata(1.0, OnPropertyChange),
                                        ValidateRevolutionsPerSecond);

        public static DependencyProperty ContentScaleProperty =
            DependencyProperty.Register("ContentScale",
                                        typeof(double),
                                        typeof(ContentSpinner),
                                        new PropertyMetadata(1.0, OnPropertyChange),
                                        ValidateContentScale);

        #endregion

        static ContentSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentSpinner), new FrameworkPropertyMetadata(typeof(ContentSpinner)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSpinner"/> class.
        /// </summary>
        public ContentSpinner()
        {
            Loaded += (o, args) => StartAnimation();
            SizeChanged += (o, args) => RestartAnimation();
            Unloaded += (o, args) => StopAnimation();
        }

        /// <summary>
        /// Gets or sets the number of revolutions per second.
        /// </summary>
        public double RevolutionsPerSecond
        {
            get { return (double)GetValue(RevolutionsPerSecondProperty); }
            set { SetValue(RevolutionsPerSecondProperty, value); }
        }

        /// <summary>
        /// Gets or sets the number of frames per rotation.
        /// </summary>
        public int NumberOfFrames
        {
            get { return (int)GetValue(NumberOfFramesProperty); }
            set { SetValue(NumberOfFramesProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content scale.
        /// </summary>
        public double ContentScale
        {
            get { return (double)GetValue(ContentScaleProperty); }
            set { SetValue(ContentScaleProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _content = GetTemplateChild("PART_Content") as FrameworkElement;
        }

        private void StartAnimation()
        {
            if (_content == null)
                return;

            var animation = GetAnimation();

            _content.LayoutTransform = GetContentLayoutTransform();
            _content.RenderTransform = GetContentRenderTransform();

            _storyboard = new Storyboard();
            _storyboard.Children.Add(animation);

            _storyboard.Begin(this);
        }

        private void StopAnimation()
        {
            if (_storyboard != null)
            {
                _storyboard.Remove(this);
                _storyboard = null;
            }
        }

        private void RestartAnimation()
        {
            StopAnimation();
            StartAnimation();
        }

        private Transform GetContentLayoutTransform()
        {
            return new ScaleTransform(ContentScale, ContentScale);
        }

        private Transform GetContentRenderTransform()
        {
            var rotateTransform = new RotateTransform(0, _content.ActualWidth / 2 * ContentScale, _content.ActualHeight / 2 * ContentScale);
            RegisterName(ANIMATION, rotateTransform);

            return rotateTransform;
        }

        private DoubleAnimationUsingKeyFrames GetAnimation()
        {
            NameScope.SetNameScope(this, new NameScope());

            var animation = new DoubleAnimationUsingKeyFrames();

            for (int i = 0; i < NumberOfFrames; i++)
            {
                var angle = i * 360.0 / NumberOfFrames;
                var time = KeyTime.FromPercent(((double)i) / NumberOfFrames);
                DoubleKeyFrame frame = new DiscreteDoubleKeyFrame(angle, time);
                animation.KeyFrames.Add(frame);
            }

            animation.Duration = TimeSpan.FromSeconds(1 / RevolutionsPerSecond);
            animation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTargetName(animation, ANIMATION);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RotateTransform.AngleProperty));

            return animation;
        }

        #region Validation and prop change methods

        private static bool ValidateNumberOfFrames(object value)
        {
            int frames = (int)value;
            return frames > 0;
        }

        private static bool ValidateContentScale(object value)
        {
            var scale = (double)value;
            return scale > 0.0;
        }

        private static bool ValidateRevolutionsPerSecond(object value)
        {
            var rps = (double) value;
            return rps > 0.0;
        }

        private static void OnPropertyChange(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var spinner = (ContentSpinner)target;
            spinner.RestartAnimation();
        }

        #endregion
    }
}
