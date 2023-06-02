using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Gpt.Labs.ViewModels;

namespace Gpt.Labs.Helpers.Navigation
{
    public class SuspensionManager
    {
        #region Fields

        public static readonly DependencyProperty FrameSessionStateKeyProperty = DependencyProperty.RegisterAttached(
            "_FrameSessionStateKey",
            typeof(string),
            typeof(Frame),
            null);

        public static readonly DependencyProperty FrameSessionStateProperty = DependencyProperty.RegisterAttached(
            "_FrameSessionState",
            typeof(FrameState),
            typeof(Frame),
            null);

        private const string SessionStateFilename = "_sessionState.json";

        private readonly List<Frame> registeredFrames = new List<Frame>();

        #endregion

        #region Properties

        public Dictionary<string, FrameState> SessionState { get; private set; } = new Dictionary<string, FrameState>();

        #endregion

        #region Public Methods

        public void RegisterFrame(Frame frame, string sessionStateKey)
        {
            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
            {
                throw new InvalidOperationException("Frames can only be registered to one session state key");
            }

            if (frame.GetValue(FrameSessionStateProperty) != null)
            {
                throw new InvalidOperationException(
                    "Frames must be either be registered before accessing frame session state, or not registered at all");
            }

            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            this.registeredFrames.Add(frame);
            
            this.RestoreFrameNavigationState(frame);
        }

        public void UnregisterFrame(Frame frame, bool removeSessionState)
        {
            var key = (string)frame.GetValue(FrameSessionStateKeyProperty);

            if (removeSessionState)
            {
                this.SessionState.Remove(key);
            }

            this.registeredFrames.Remove(frame);

            frame.ClearValue(FrameSessionStateProperty);
            frame.ClearValue(FrameSessionStateKeyProperty);
        }

        public async Task RestoreAsync()
        {
            this.SessionState = new Dictionary<string, FrameState>();

            try
            {
                var file = await ApplicationData.Current.TemporaryFolder.GetFileAsync(SessionStateFilename);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    this.SessionState = await JsonSerializer.DeserializeAsync<Dictionary<string, FrameState>>(stream, ApplicationSettings.Instance.SerializerOptions);
                }

                foreach (var frame in this.registeredFrames)
                {
                    frame.ClearValue(FrameSessionStateProperty);
                    this.RestoreFrameNavigationState(frame);
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }
        
        public async Task SaveAsync()
        {
            try
            {
                foreach (var frame in this.registeredFrames)
                {
                    this.SaveFrameNavigationState(frame);

                }

                var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                               SessionStateFilename,
                               CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    await JsonSerializer.SerializeAsync(stream, this.SessionState, ApplicationSettings.Instance.SerializerOptions);
                    await stream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }
        
        public FrameState SessionStateForFrame(Frame frame)
        {
            var frameState = (FrameState)frame.GetValue(FrameSessionStateProperty);

            if (frameState == null)
            {
                var frameSessionKey = (string)frame.GetValue(FrameSessionStateKeyProperty);
                if (frameSessionKey != null)
                {
                    if (!this.SessionState.ContainsKey(frameSessionKey))
                    {
                        this.SessionState[frameSessionKey] = new FrameState();
                    }

                    frameState = (FrameState)this.SessionState[frameSessionKey];
                }
                else
                {
                    frameState = new FrameState();
                }

                frame.SetValue(FrameSessionStateProperty, frameState);
            }

            return frameState;
        }
                
        public void SaveFrameNavigationState(Frame frame)
        {
            var frameState = this.SessionStateForFrame(frame);
            frameState.Navigation = frame.GetNavigationState();
        }

        #endregion

        #region Private Methods

        private void RestoreFrameNavigationState(Frame frame)
        {
            var frameState = this.SessionStateForFrame(frame);
            if (!string.IsNullOrEmpty(frameState.Navigation))
            {
                frame.SetNavigationState(frameState.Navigation);
            }
        }

        #endregion
    }
}
