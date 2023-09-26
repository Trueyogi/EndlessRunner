// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity.PoseTracking
{
    public class PoseTrackingSolution : ImageSourceSolution<PoseTrackingGraph>
    {
        private IList<NormalizedLandmark> _currentTarget;

        public PoseTrackingGraph.ModelComplexity modelComplexity
        {
            get => graphRunner.modelComplexity;
            set => graphRunner.modelComplexity = value;
        }

        public bool smoothLandmarks
        {
            get => graphRunner.smoothLandmarks;
            set => graphRunner.smoothLandmarks = value;
        }

        public bool enableSegmentation
        {
            get => graphRunner.enableSegmentation;
            set => graphRunner.enableSegmentation = value;
        }

        public bool smoothSegmentation
        {
            get => graphRunner.smoothSegmentation;
            set => graphRunner.smoothSegmentation = value;
        }

        public float minDetectionConfidence
        {
            get => graphRunner.minDetectionConfidence;
            set => graphRunner.minDetectionConfidence = value;
        }

        public float minTrackingConfidence
        {
            get => graphRunner.minTrackingConfidence;
            set => graphRunner.minTrackingConfidence = value;
        }

        protected override void SetupScreen(ImageSource imageSource)
        {
            base.SetupScreen(imageSource);
            //_worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
        }

        protected override void OnStartRun()
        {
            if (!runningMode.IsSynchronous())
            {
                graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
                graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
                graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
                graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
                graphRunner.OnRoiFromLandmarksOutput += OnRoiFromLandmarksOutput;
            }

            var imageSource = ImageSourceProvider.ImageSource;
            /*SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
            SetupAnnotationController(_poseLandmarksAnnotationController, imageSource);
            SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
            SetupAnnotationController(_segmentationMaskAnnotationController, imageSource);
            _segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
            SetupAnnotationController(_roiFromLandmarksAnnotationController, imageSource);*/
        }

        protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
        {
            graphRunner.AddTextureFrameToInputStream(textureFrame);
        }

        protected override IEnumerator WaitForNextValue()
        {
            Detection poseDetection = null;
            NormalizedLandmarkList poseLandmarks = null;
            LandmarkList poseWorldLandmarks = null;
            ImageFrame segmentationMask = null;
            NormalizedRect roiFromLandmarks = null;

            if (runningMode == RunningMode.Sync)
            {
                var _ = graphRunner.TryGetNext(out poseDetection, out poseLandmarks, out poseWorldLandmarks,
                    out segmentationMask, out roiFromLandmarks, true);
            }
            else if (runningMode == RunningMode.NonBlockingSync)
            {
                yield return new WaitUntil(() => graphRunner.TryGetNext(out poseDetection, out poseLandmarks,
                    out poseWorldLandmarks, out segmentationMask, out roiFromLandmarks, false));
            }

            if (poseLandmarks?.Landmark != null)
            {
                //ProcessOutputsForHands(poseLandmarks?.Landmark);
            }

            /*_poseDetectionAnnotationController.DrawNow(poseDetection);
            _poseLandmarksAnnotationController.DrawNow(poseLandmarks);
            _poseWorldLandmarksAnnotationController.DrawNow(poseWorldLandmarks);
            _segmentationMaskAnnotationController.DrawNow(segmentationMask);
            _roiFromLandmarksAnnotationController.DrawNow(roiFromLandmarks);*/
        }

        private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs)
        {
        }

        private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
        {
            UpdateCurrentTarget(eventArgs.value?.Landmark, ref MLOutputController.instance.currentTarget);
        }

        private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs)
        {
        }

        private void OnSegmentationMaskOutput(object stream, OutputEventArgs<ImageFrame> eventArgs)
        {
        }

        private void OnRoiFromLandmarksOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs)
        {
        }

        private void UpdateCurrentTarget<TValue>(TValue newTarget, ref TValue currentTarget)
        {
            if (IsTargetChanged(newTarget, currentTarget))
            {
                currentTarget = newTarget;
                MLOutputController.instance.isStale = true;
            }
        }

        private bool IsTargetChanged<TValue>(TValue newTarget, TValue currentTarget)
        {
            return currentTarget != null || newTarget != null;
        }
    }
}