using System;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Ostsoft.Games.SuperKinectroid
{
    public class SuperKinectroid
    {
        private Usb2Snes _usb2Snes = new Usb2Snes();
        private Stopwatch _stopwatch = Stopwatch.StartNew();
        private Buttons _buttons = new Buttons();
        private int activeBody = -1;
        private int lastButtons = -1;
        private ShootingMode _shootingMode = ShootingMode.LOW_ACCURACY;
        private Stopwatch _toggleStopwatch = Stopwatch.StartNew();
        private Stopwatch _delayOnStopwatch = Stopwatch.StartNew();
        private string _message = "Super Kinectroid!";
        private Stopwatch _messageTimer = Stopwatch.StartNew();
        private OSD osd = new OSD();

        private DelayedToggle thumbUp = new DelayedToggle();
        private DelayedToggle thumbLeft = new DelayedToggle();
        private DelayedToggle thumbDown = new DelayedToggle();

        public SuperKinectroid()
        {
            _stopwatch.Stop();
        }

        public void UpdateBodies(Body[] bodies, DrawingImage zoneSource)
        {
            _buttons.unpressAll();

            for (var index = 0; index < bodies.Length; index++)
            {
                var body = bodies[index];
                if (body.IsTracked &&
                    body.HandLeftConfidence == TrackingConfidence.High &&
                    body.HandLeftState == HandState.Lasso &&
                    body.HandRightConfidence == TrackingConfidence.High &&
                    body.HandRightState == HandState.Lasso)
                {
                    osd.displayMessage("Body " + index + " is now in control");
                    activeBody = index;
//                    activeBodies.Add(index);
                }
            }

            for (var index = 0; index < bodies.Length; index++)
            {
                var body = bodies[index];
                if (!body.IsTracked && activeBody == index)
                {
                    activeBody = -1;
                    osd.displayMessage("Controls are now released!");
                    continue;
                }

                if (!body.IsTracked || activeBody != index)
                {
                    continue;
                }

                var halfASpine = body.Joints[JointType.SpineMid].Position.Y -
                                 body.Joints[JointType.SpineBase].Position.Y;

                var leftHandBelowSpineBase =
                    body.Joints[JointType.HandLeft].Position.Y <
                    body.Joints[JointType.SpineBase].Position.Y + (halfASpine / 8);
                var rightHandBelowSpineBase =
                    body.Joints[JointType.HandRight].Position.Y <
                    body.Joints[JointType.SpineBase].Position.Y + (halfASpine / 8);
                var anyHandBelowSpineBase = leftHandBelowSpineBase || rightHandBelowSpineBase;
                var bothHandBelowSpineBase = leftHandBelowSpineBase && rightHandBelowSpineBase;


                var leftHandAboveNeck =
                    body.Joints[JointType.HandLeft].Position.Y > body.Joints[JointType.Neck].Position.Y;
                var rightHandAboveNeck =
                    body.Joints[JointType.HandRight].Position.Y > body.Joints[JointType.Neck].Position.Y;
                var anyHandAboveNeck = leftHandAboveNeck || rightHandAboveNeck;
                var bothHandAboveNeck = leftHandAboveNeck && rightHandAboveNeck;


                var leftHandAboveHead =
                    body.Joints[JointType.HandLeft].Position.Y > body.Joints[JointType.Head].Position.Y;
                var rightHandAboveHead =
                    body.Joints[JointType.HandRight].Position.Y > body.Joints[JointType.Head].Position.Y;
                var anyHandAboveHead = leftHandAboveHead || rightHandAboveHead;
                var bothHandAboveHead = leftHandAboveHead && rightHandAboveHead;

                var shoulderWidth = body.Joints[JointType.ShoulderRight].Position.X -
                                    body.Joints[JointType.ShoulderLeft].Position.X;


                var leftHandIndicatingMovingRight = body.Joints[JointType.HandLeft].Position.X >
                                                    body.Joints[JointType.SpineMid].Position.X;

                var leftHandIndicatingMovingLeft = body.Joints[JointType.HandLeft].Position.X <
                                                   body.Joints[JointType.ShoulderLeft].Position.X -
                                                   shoulderWidth;

                var rightHandIndicatingMovingLeft = body.Joints[JointType.HandRight].Position.X <
                                                    body.Joints[JointType.SpineMid].Position.X;

                var rightHandIndicatingMovingRight = body.Joints[JointType.HandRight].Position.X >
                                                     body.Joints[JointType.ShoulderRight].Position.X +
                                                     shoulderWidth;

                var leftHandClosed = body.HandLeftState == HandState.Closed
                                     && body.Joints[JointType.HandLeft].TrackingState == TrackingState.Tracked;

                var rightHandClosed = body.HandRightState == HandState.Closed
                                      && body.Joints[JointType.HandRight].TrackingState == TrackingState.Tracked;

                var kneeTrigger = body.Joints[JointType.SpineBase].Position.Y - halfASpine / 3 * 2;

                var leftFeetAboveSpineBase = body.Joints[JointType.KneeLeft].Position.Y > kneeTrigger;

                var rightFeetAboveSpineBase = body.Joints[JointType.KneeRight].Position.Y > kneeTrigger;

                var leftHandInFront = body.Joints[JointType.HandLeft].Position.Z <
                                      body.Joints[JointType.ShoulderLeft].Position.Z - 0.30;

                var rightHandInFront = body.Joints[JointType.HandRight].Position.Z <
                                       body.Joints[JointType.ShoulderRight].Position.Z - 0.30;

                var rightHandFarInFront = body.Joints[JointType.HandRight].Position.Z <
                                          body.Joints[JointType.ShoulderRight].Position.Z - 0.50;

                var rightHandStraightFront = body.Joints[JointType.ShoulderRight].Position.X + shoulderWidth / 2 >
                                             body.Joints[JointType.HandRight].Position.X
                                             && body.Joints[JointType.ShoulderRight].Position.X - shoulderWidth / 2 <
                                             body.Joints[JointType.HandRight].Position.X
                                             && !rightHandAboveNeck
                                             && !rightHandBelowSpineBase;

                var rightThumpPointingUp = body.Joints[JointType.HandRight].Position.Y <
                                           body.Joints[JointType.ThumbRight].Position.Y - 0.05;

                var rightThumpPointingDown = body.Joints[JointType.HandRight].Position.Y >
                                             body.Joints[JointType.ThumbRight].Position.Y + 0.05;

                var rightThumpPointingLeft = body.Joints[JointType.HandRight].Position.X >
                                             body.Joints[JointType.ThumbRight].Position.X + 0.03 &&
                                             !rightThumpPointingUp && !rightThumpPointingDown;


                // Moving left or right
                if (!leftHandIndicatingMovingLeft || !rightHandIndicatingMovingRight)
                {
                    if (leftHandIndicatingMovingLeft && rightHandIndicatingMovingLeft)
                    {
                        // Running left
                        _buttons.pressLeft();
                        _buttons.pressB();
                    }
                    else if (leftHandIndicatingMovingRight && rightHandIndicatingMovingRight)
                    {
                        // Running right
                        _buttons.pressRight();
                        _buttons.pressB();
                    }
                    else if (leftHandIndicatingMovingLeft)
                    {
                        // Walking left
                        _buttons.pressLeft();
                    }
                    else if (rightHandIndicatingMovingRight)
                    {
                        // Walking right
                        _buttons.pressRight();
                    }
                }

                // Jumping or pointing upwards
                if (bothHandAboveHead)
                {
                    _stopwatch.Start();
                    if (_stopwatch.ElapsedMilliseconds > 1500)
                    {
                        _buttons.pressUp();
                    }
                }
                else
                {
                    _stopwatch.Reset();
                }

                if (anyHandAboveNeck && !_buttons.pressingUp())
                {
                    _buttons.pressA();
                }

                if (anyHandBelowSpineBase && !_buttons.pressingUp())
                {
                    _buttons.pressDown();
                }

                // Shooting
                if (_shootingMode != ShootingMode.DISABLED
                    && (leftHandClosed || rightHandClosed)
                    && (_shootingMode == ShootingMode.LOW_ACCURACY
                        || leftHandClosed && body.HandLeftConfidence == TrackingConfidence.High
                        || rightHandClosed && body.HandRightConfidence == TrackingConfidence.High)
                )
                {
                    _buttons.pressX();
                }

                if (leftFeetAboveSpineBase)
                {
                    _buttons.pressStart();
                }

                if (rightFeetAboveSpineBase)
                {
                    _buttons.pressY();
                }


                thumbUp.setActive(rightHandFarInFront && rightHandStraightFront && !leftHandInFront &&
                                  rightThumpPointingUp && !rightThumpPointingLeft && !rightThumpPointingDown);

                if (thumbUp.isTrigged())
                {
                    osd.displayMessage("Shooting is now in LOW accuracy mode");
                    _shootingMode = ShootingMode.LOW_ACCURACY;
                }
                else if (thumbUp.isDelayOnActive())
                {
                    osd.displayMessage("Keep pointing up to enable LOW accuracy shooting", 1000);
                }

                thumbLeft.setActive(rightHandFarInFront && rightHandStraightFront && !leftHandInFront &&
                                    rightThumpPointingLeft);
                if (thumbLeft.isTrigged())
                {
                    osd.displayMessage("Shooting is now in HIGH accuracy mode");
                    _shootingMode = ShootingMode.HIGH_ACCURACY;
                }
                else if (thumbLeft.isDelayOnActive())
                {
                    osd.displayMessage("Keep pointing left to enable HIGH accuracy shooting", 1000);
                }

                thumbDown.setActive(rightHandFarInFront && rightHandStraightFront && !leftHandInFront &&
                                    rightThumpPointingDown && !rightThumpPointingLeft && !rightThumpPointingUp);
                if (thumbDown.isTrigged())
                {
                    osd.displayMessage("Shooting now disabled");
                    _shootingMode = ShootingMode.DISABLED;
                }
                else if (thumbDown.isDelayOnActive())
                {
                    osd.displayMessage("Keep pointing down to DISABLE shooting", 1000);
                }
            }


            RefreshButtons();

            osd.update(zoneSource);
        }

        private void printMessage(String message)
        {
            Console.WriteLine("!");
            Console.WriteLine(message);
            Console.WriteLine("!");
        }

        private void RefreshButtons()
        {
            if (_buttons.buttons == lastButtons)
                return;

            Console.WriteLine("Button pressed: " + _buttons.ToString() + " Value: " + _buttons.buttons);


            var bytes = new byte[2];
            bytes[0] = (byte) (_buttons.buttons >> 8);
            bytes[1] = (byte) _buttons.buttons;

            _usb2Snes.Write(0xE01CFA, bytes);

            lastButtons = _buttons.buttons;
        }
    }

    class Buttons
    {
        public int buttons = 0;

        public void unpressAll() => buttons = 0;

        public void pressY() => buttons = buttons | 0x0040;
        public void pressB() => buttons = buttons | 0x0080;
        public void pressX() => buttons = buttons | 0x4000;
        public void pressA() => buttons = buttons | 0x8000;

        public void pressL() => buttons = buttons | 0x2000;
        public void pressR() => buttons = buttons | 0x1000;

        public void pressUp() => buttons = buttons | 0x0008;
        public void pressDown() => buttons = buttons | 0x0004;
        public void pressLeft() => buttons = buttons | 0x0002;
        public void pressRight() => buttons = buttons | 0x0001;

        public void pressStart() => buttons = buttons | 0x0010;
        public void pressSelect() => buttons = buttons | 0x0020;


        public bool pressingY() => (buttons & 0x0040) != 0;
        public bool pressingB() => (buttons & 0x0080) != 0;
        public bool pressingX() => (buttons & 0x4000) != 0;
        public bool pressingA() => (buttons & 0x8000) != 0;
        public bool pressingL() => (buttons & 0x2000) != 0;
        public bool pressingR() => (buttons & 0x1000) != 0;

        public bool pressingUp() => (buttons & 0x0008) != 0;
        public bool pressingDown() => (buttons & 0x0004) != 0;
        public bool pressingLeft() => (buttons & 0x0002) != 0;
        public bool pressingRight() => (buttons & 0x0001) != 0;

        public bool pressingStart() => (buttons & 0x0010) != 0;
        public bool pressingSelect() => (buttons & 0x0020) != 0;

        public override string ToString()
        {
            return (pressingY() ? "Y" : " ") +
                   (pressingB() ? "B" : " ") +
                   (pressingX() ? "X" : " ") +
                   (pressingA() ? "A" : " ") +
                   (pressingL() ? "L" : " ") +
                   (pressingR() ? "R" : " ") +
                   (pressingUp() ? "U" : " ") +
                   (pressingDown() ? "D" : " ") +
                   (pressingLeft() ? "L" : " ") +
                   (pressingRight() ? "R" : " ") +
                   (pressingStart() ? "Start" : "     ") +
                   (pressingSelect() ? "Select" : "      ");
        }
    }

    public enum ShootingMode
    {
        HIGH_ACCURACY,
        LOW_ACCURACY,
        DISABLED
    }
}