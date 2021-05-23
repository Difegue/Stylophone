// This file has been autogenerated from a class added in the UI designer.

using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// https://medium.com/flawless-app-stories/simple-progress-ring-view-in-ios-166896fecf6b
    /// </summary>
	public partial class UICircularProgressView : UIView
	{
		public UICircularProgressView (IntPtr handle) : base (handle)
		{
		}
     
        private CAShapeLayer _backgroundCircle = new CAShapeLayer();
        private CAShapeLayer _progressCircle = new CAShapeLayer();

        private UIColor _backgroundCircleColor = UIColor.SystemGray4Color;
        public UIColor BackgroundCircleColor
        {
            get => _backgroundCircleColor;
            set {
                _backgroundCircleColor = value;
                _backgroundCircle.StrokeColor = value.CGColor;
            }
        }

        private UIColor _fillColor = UIColor.White;
        public UIColor FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                _progressCircle.StrokeColor = value.CGColor;
            }
        }

        private float _lineWidth = 8;
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                _progressCircle.LineWidth = value;
            }
        }

        private float _progress = 0;
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                UpdateProgress(value);
            }
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            ConfigureView();
        }

        private void ConfigureView()
        {
            DrawBackgroundCircle();
            DrawProgressCircle();
        }

        private void DrawBackgroundCircle()
        {
            _backgroundCircle = new CAShapeLayer();
            var centerPoint = new CGPoint(Bounds.Width / 2, Bounds.Width / 2);
            var circleRadius = Bounds.Width / 2;

            var circlePath = UIBezierPath.Create();
            circlePath.AddArc(centerPoint, circleRadius, (float)(-0.5 * Math.PI), (float)(1.5 * Math.PI), true);

            _backgroundCircle.Path = circlePath.CGPath;
            _backgroundCircle.StrokeColor = BackgroundCircleColor.CGColor;
            _backgroundCircle.FillColor = UIColor.Clear.CGColor;

            _backgroundCircle.LineWidth = LineWidth;
            _backgroundCircle.LineCap = new NSString("round");
            _backgroundCircle.LineJoin = new NSString("round");
            _backgroundCircle.StrokeStart = 0;
            _backgroundCircle.StrokeEnd = 1;

            Layer.AddSublayer(_backgroundCircle);
        }

        private void DrawProgressCircle()
        {
            _progressCircle = new CAShapeLayer();
            var centerPoint = new CGPoint(Bounds.Width / 2, Bounds.Width / 2);
            var circleRadius = Bounds.Width / 2;

            var circlePath = UIBezierPath.Create();
            circlePath.AddArc(centerPoint, circleRadius, (float)(-0.5 * Math.PI), (float)(1.5 * Math.PI), true);

            _progressCircle.Path = circlePath.CGPath;
            _progressCircle.StrokeColor = FillColor.CGColor;
            _progressCircle.FillColor = UIColor.Clear.CGColor;

            _progressCircle.LineWidth = LineWidth;
            _progressCircle.LineCap = new NSString("round");
            _progressCircle.LineJoin = new NSString("round");
            _progressCircle.StrokeStart = 0;
            _progressCircle.StrokeEnd = Progress;

            Layer.AddSublayer(_progressCircle);
        }

        private void UpdateProgress(float progress)
        {
            _progressCircle.StrokeEnd = progress / 100;
        }

    }
}
