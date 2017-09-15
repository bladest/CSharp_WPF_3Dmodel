using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace hyrad
{
	
	public partial class MainWindow : Window
	{
		private Point3D _center;
		PerspectiveCamera myPCamera;
		Point3D newPostition;
		double mouseDeltaFactor = 1;
		Point mouseLastPosition;
		int num=0;

		private double NewMouseX = 0.0;   
		private double NewMouseY = 0.0;                      //按下右键时坐标
		private double TmpOldMouseX = 0.0;
		private double TmpOldMouseY = 0.0;

		public MainWindow()
		{
			this.InitializeComponent();
			myPCamera = new PerspectiveCamera();
			myPCamera.Position = new Point3D(0, 0, 400);       //表示摄像机“从哪看”的位置，当前位置是正上方200
			myPCamera.LookDirection = new Vector3D(0, 0, -1);   //表示摄像机“朝哪看”的方向，当前方向是朝正下方看
			myPCamera.UpDirection = new Vector3D(0, -1, 0);
			myPCamera.FieldOfView = 1000;
			hyrad.Camera = myPCamera;
			DirectionalLight myDirectionalLight = new DirectionalLight();
			myDirectionalLight.Color = Colors.White;
			myDirectionalLight.Direction = new Vector3D(0.61, 0.5, 0.61);
			YawWithDefaultCenter(false, 2);

			#region '图形硬件功能级别显示
			int renderingTier = (RenderCapability.Tier >> 16);
			title.Content = renderingTier;
			/*if (2 == renderingTier)
				label.Content = "大多数图形功能都使用图形硬件加速";
			if (1 == renderingTier)
				label.Content =  "某些图形功能使用图形硬件加速";
			if (0 == renderingTier)
				label.Content =  "无图形硬件加速";*/
			#endregion

			#region '窗体事件添加
			hyrad.MouseLeave += hyrad_MouseLeave;                //添加一系列事件
			hyrad.MouseWheel += hyrad_MouseWheel;
			hyrad.MouseLeave += hyrad_MouseLeave;
			hyrad.MouseLeftButtonDown += hyrad_MouseLeftButtonDown;
			Window.MouseRightButtonDown += Window_MouseRightButtonDown;
			Window.MouseWheel += window_MouseWheel;
			Window.MouseMove += Window_MouseMove;
			Window.MouseDoubleClick += Window_MouseDoubleClick;
			#endregion
		}

		private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			num += 1;
			if (num >= 2)
			{
				Close();
			}
			else
			{
				MeasureModel(RootGeometryContainer);
				CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
			}
		}

		private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			myPCamera.Position = new Point3D(0, 0, 400);       //表示摄像机“从哪看”的位置，当前位置是正上方200
			myPCamera.LookDirection = new Vector3D(0, 0, -1);   //表示摄像机“朝哪看”的方向，当前方向是朝正下方看
			myPCamera.UpDirection = new Vector3D(0, -1, 0);
			YawWithDefaultCenter(false, 2);
			NewMouseX = e.GetPosition(hyrad).X;
			NewMouseY = e.GetPosition(hyrad).Y;
		}
		
		private void hyrad_MouseLeave(object sender, MouseEventArgs e)
		{
			hyrad.Effect = null;
		}
		
		#region   模型左键按下
		private void hyrad_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//mouseLastPosition = e.GetPosition(this);
		}
		#endregion

		#region 窗口鼠标移动
		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			#region' 左键按下事件
			if (Mouse.LeftButton == MouseButtonState.Pressed)//如果按下鼠标左键
			{
				//myPCamera.Position = new Point3D(0, 0, 400);
				Point newMousePosition = e.GetPosition(this);  //当前鼠标位置
				if (mouseLastPosition.X != newMousePosition.X)
				{
					HorizontalTransform(mouseLastPosition.X < newMousePosition.X, mouseDeltaFactor);//水平变换
				}
				if (mouseLastPosition.Y != newMousePosition.Y)
				{
					//进行垂直旋转
					VerticalTransform(mouseLastPosition.Y > newMousePosition.Y, mouseDeltaFactor);//垂直变换 
				}
				mouseLastPosition = newMousePosition;           //更新位置，非常重要
			}
			#endregion

			#region' 右键按下事件
			if (Mouse.RightButton == MouseButtonState.Pressed)
			{
				//Point newMousePosition = e.GetPosition(this);
				TmpOldMouseX = e.GetPosition(hyrad).X;
				TmpOldMouseY = e.GetPosition(hyrad).Y;
				myPCamera.Position = new Point3D(NewMouseX - TmpOldMouseX, TmpOldMouseY - NewMouseY, 400);
			}
			#endregion
		}
		#endregion

		#region   缩放、旋转代码
		//鼠标滚轮缩放
		private void window_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			double scaleFactor = 120;
			//120 near ,   -120 far
			System.Diagnostics.Debug.WriteLine(e.Delta.ToString());
			Point3D currentPosition = myPCamera.Position;
			Vector3D lookDirection = myPCamera.LookDirection;
			//new Vector3D(myPCamera.LookDirection.X, myPCamera.LookDirection.Y,  myPCamera.LookDirection.Z);
			lookDirection.Normalize();

			lookDirection *= scaleFactor;

			if (e.Delta == 120)//getting near   e.Delta越大，缩放级别就越高
			{
				//myPCamera.FieldOfView /= 1.2;
				if ((currentPosition.X + lookDirection.X) * currentPosition.X > 0)
				{
					currentPosition += lookDirection;
				}
			}
			if (e.Delta == -120)//getting far
			{
				//myPCamera.FieldOfView *= 1.2;
				currentPosition -= lookDirection;
			}

			Point3DAnimation positionAnimation = new Point3DAnimation();
			positionAnimation.BeginTime = new TimeSpan(0, 0, 0);
			positionAnimation.Duration = TimeSpan.FromMilliseconds(100);
			positionAnimation.To = currentPosition;
			positionAnimation.From = myPCamera.Position;
			positionAnimation.Completed += new EventHandler(positionAnimation_Completed);
			myPCamera.BeginAnimation(PerspectiveCamera.PositionProperty, positionAnimation, HandoffBehavior.Compose);
		}
		//鼠标滚轮缩放模型
		private void hyrad_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			double scaleFactor = 120;
			//120 near ,   -120 far
			//System.Diagnostics.Debug.WriteLine(e.Delta.ToString());
			Point3D currentPosition = myPCamera.Position;
			Vector3D lookDirection = myPCamera.LookDirection;//new Vector3D(camera.LookDirection.X, camera.LookDirection.Y, camera.LookDirection.Z);
			lookDirection.Normalize();

			lookDirection *= scaleFactor;

			if (e.Delta == 120)//getting near
			{
				//myPCamera.FieldOfView /= 1.2;
				if ((currentPosition.X + lookDirection.X) * currentPosition.X > 0)
				{
					currentPosition += lookDirection;
				}
			}
			if (e.Delta == -120)//getting far
			{
				//myPCamera.FieldOfView *= 1.2;
				currentPosition -= lookDirection;
			}

			Point3DAnimation positionAnimation = new Point3DAnimation();
			positionAnimation.BeginTime = new TimeSpan(0, 0, 0);
			positionAnimation.Duration = TimeSpan.FromMilliseconds(100);
			positionAnimation.To = currentPosition;
			positionAnimation.From = myPCamera.Position;
			positionAnimation.Completed += new EventHandler(positionAnimation_Completed);
			myPCamera.BeginAnimation(PerspectiveCamera.PositionProperty, positionAnimation, HandoffBehavior.Compose);
		}
		//动画结束
		void positionAnimation_Completed(object sender, EventArgs e)
		{
			Point3D position = myPCamera.Position;
			myPCamera.BeginAnimation(PerspectiveCamera.PositionProperty, null);
			//PerspectiveCamera简化为ProjectionCamera
			myPCamera.Position = position;
		}

		// 垂直变换
		private void VerticalTransform(bool upDown, double angleDeltaFactor)
		{
			Vector3D postion = new Vector3D(myPCamera.Position.X, myPCamera.Position.Y, myPCamera.Position.Z);
			Vector3D rotateAxis = Vector3D.CrossProduct(postion, myPCamera.UpDirection);
			RotateTransform3D rt3d = new RotateTransform3D();
			AxisAngleRotation3D rotate = new AxisAngleRotation3D(rotateAxis, angleDeltaFactor * (upDown ? 1 : -1));
			rt3d.Rotation = rotate;
			Matrix3D matrix = rt3d.Value;
			Point3D newPostition = matrix.Transform(myPCamera.Position);
			//myPCamera.Position = newPostition;
			myPCamera.Position = newPostition;
			myPCamera.LookDirection = myPCamera.LookDirection = _center - newPostition;
			Vector3D newUpDirection = Vector3D.CrossProduct(myPCamera.LookDirection, rotateAxis);
			newUpDirection.Normalize();
			myPCamera.UpDirection = newUpDirection;
		}
		// 水平变换：
		private void HorizontalTransform(bool leftRight, double angleDeltaFactor)
		{
			Vector3D postion = new Vector3D(myPCamera.Position.X, myPCamera.Position.Y, myPCamera.Position.Z);
			Vector3D rotateAxis = myPCamera.UpDirection;
			RotateTransform3D rt3d = new RotateTransform3D();
			AxisAngleRotation3D rotate = new AxisAngleRotation3D(rotateAxis, angleDeltaFactor * (leftRight ? 1 : -1));
			rt3d.Rotation = rotate;
			Matrix3D matrix = rt3d.Value;
			newPostition = matrix.Transform(myPCamera.Position);
			myPCamera.Position = newPostition;
			myPCamera.LookDirection = myPCamera.LookDirection = _center - newPostition;
		}
		//自动旋转代码
		public void MeasureModel(ModelVisual3D model)
		{
			var rect3D = Rect3D.Empty;
			UnionRect(model, ref rect3D);

			_center = new Point3D((rect3D.X + rect3D.SizeX / 2), (rect3D.Y + rect3D.SizeY / 2),
								  (rect3D.Z + rect3D.SizeZ / 2));

			double radius = (_center - rect3D.Location).Length;
			Point3D position = _center;
			position.Z += radius * 2;
			position.X = position.Z;
			/*MyPCamera.Position = position;
			camera.LookDirection = _center - position;
			camera.NearPlaneDistance = radius / 100;
			camera.FarPlaneDistance = radius * 100;*/
		}
		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			Yaw(false, 2);
			YawWithDefaultCenter(false, 2);
		}

		public void YawWithDefaultCenter(bool leftRight, double angleDeltaFactor)
		{
			var axis = new AxisAngleRotation3D(myPCamera.UpDirection, leftRight ? angleDeltaFactor : -angleDeltaFactor);
			var rt3D = new RotateTransform3D(axis);
			Matrix3D matrix3D = rt3D.Value;
			Point3D point3D = myPCamera.Position;
			Point3D position = matrix3D.Transform(point3D);
			myPCamera.Position = position;
			myPCamera.LookDirection = myPCamera.LookDirection = _center - position;
		}
		private void UnionRect(ModelVisual3D model, ref Rect3D rect3D)
		{
			for (int i = 0; i < model.Children.Count; i++)
			{
				var child = model.Children[i] as ModelVisual3D;
				UnionRect(child, ref rect3D);

			}
			if (model.Content != null)
				rect3D.Union(model.Content.Bounds);
		}

		public void Yaw(bool leftRight, double angleDeltaFactor)
		{

			var axis = new AxisAngleRotation3D(myPCamera.UpDirection, leftRight ? angleDeltaFactor : -angleDeltaFactor);
			var rt3D = new RotateTransform3D(axis) { CenterX = _center.X, CenterY = _center.Y, CenterZ = _center.Z };
			Matrix3D matrix3D = rt3D.Value;
			Point3D point3D = myPCamera.Position;
			Point3D position = matrix3D.Transform(point3D);
			myPCamera.Position = position;
			myPCamera.LookDirection = myPCamera.LookDirection = _center - position;
		}
		//自动旋转代码结束
		#endregion
	}
}