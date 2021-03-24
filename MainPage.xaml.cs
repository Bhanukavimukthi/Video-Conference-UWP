using Eminutes.UWP.Helpers;
using Eminutes.UWP.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.MixedReality.WebRTC;
using Nancy.Json;
using Newtonsoft.Json;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Eminutes.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class userData
    {
        private string orgPermission { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        const string ConnectionString = "Data Source=DESKTOP-494IGS0\\SQLEXPRESS;Initial Catalog=EcoDb;Persist Security Info=True;User ID=sa;Password=123";

        string Email;
        string roomId;
        string name;
        string orgPermission;
        private MediaElement element;
        private MediaElement remoteElement;
        LocalClient localClient;
        HubConnection connection;
        private PeerConnection _peerConnection;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<Rootobject> _meetings;
        public ObservableCollection<Rootobject> initiatedMeetings
        {
            get { return _meetings; }
            set
            {
                _meetings = value;
                OnPropertyChanged(nameof(initiatedMeetings));
            }
        }
        //List<Rootobject> initiatedMeetings;

        private IAPIHelper _apiHelper;


        public static List<remoteVideo> remoteVideo = new List<remoteVideo>();


        public static List<init> allLocalClients = new List<init>();
        int numberOfUsers = 0;



        public class init
        {
            public object localclient { get; set; }
            public string stream { get; set; }

        }

        public MainPage(/*IAPIHelper apiHelper*/)
        {
            _apiHelper = new APIHelper();
            this.InitializeComponent();
            InitilizeHub();
            this.Loaded += OnLoaded;
            Application.Current.Suspending += App_Suspending;

            initiatedMeetings = new ObservableCollection<Rootobject>();

            connection.On<string>("connectionID", (id) =>
            {
                Debug.WriteLine("Connection ID" + id);
            });



            this.connection.On<string, string, string>("ReceiveOffer", async (offer, peerUser, device) =>
            {
                Debug.WriteLine("STEP 3: offer received");
                dynamic sessionDescription = JsonConvert.DeserializeObject(offer);
                //check GetValueOrDefault();
                RTCSdpType a = (sessionDescription.type);
                string b = sessionDescription.sdp;
                var _offer = new RTCSessionDescription(a, b);
                Debug.WriteLine("this is the offer" + _offer);

                var remote = new MediaElement();
                remote.Height = 400;
                remote.Width = 400;
                videoList.Children.Add(remote);

                var peering = new Peering(peerUser, connection, remote, Window.Current.Dispatcher);

                peering.createPeerConnection(peerUser, localClient.allStream);

                // insert the offer as the remote description
                try
                {
                    await peering.peerConnection.SetRemoteDescription(_offer);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                // ONLY THEN we create answer
                var answer = await peering.createAnswer();

                localClient.peerings.Add(peering);
                var json = new JavaScriptSerializer().Serialize(answer);

                await this.connection.InvokeAsync("SendAnswer", json, peerUser, "windows");
                Debug.WriteLine("STEP 5: answer sended.");


                //display users for component
                remote.Name = peering.generatedId;
                remoteVideo.Add(new remoteVideo { remote = remote, id = peering.generatedId });

            });

            this.connection.On<string, string, string>("ReceiveAnswer", async (answer, peerUser, device) =>
            {
                Debug.WriteLine("Answer recieved");

                Peering peering = localClient.getPeeringByPeerClient(peerUser);

                // insert the answer as the remote description
                dynamic sessionDescription = JsonConvert.DeserializeObject(answer);
                RTCSdpType a = sessionDescription.type;
                string b = sessionDescription.sdp;
                var _answer = new RTCSessionDescription(a, b);
                await peering.peerConnection.SetRemoteDescription(_answer);

                Debug.WriteLine("PEERING ACHIEVED");
            });


            this.connection.On<string, string, string>("AddIceCandidate", async (iceCandidate, peer, device) =>
            {
                dynamic _IceCandidate = JsonConvert.DeserializeObject(iceCandidate);
                RTCIceCandidate _iceCandidate;
                string a, b;
                ushort c;
                if (device == "web")
                {
                    a = _IceCandidate.candidate;
                    b = _IceCandidate.sdpMid;
                    c = _IceCandidate.sdpMLineIndex;
                    //RTCIceCandidate _iceCandidate;
                    _iceCandidate = new RTCIceCandidate(a, b, c);
                }
                else
                {
                    a = _IceCandidate.Candidate;
                    b = _IceCandidate.SdpMid;
                    c = _IceCandidate.SdpMLineIndex;
                    _iceCandidate = new RTCIceCandidate(a, b, c);
                }

                Peering peering = localClient.getPeeringByPeerClient(peer);

                try
                {
                    await peering.peerConnection.AddIceCandidate(_iceCandidate);
                    Debug.WriteLine("Ice Candidate added");
                }
                catch (Exception err)
                {

                }
            });

            this.connection.On<string, string, string>("requestSent", (name, email, response) =>
            {
                //var Msginterface: IRequestInterface = { name: name, email: email, response: response }
                //this.requsetingUser.emit(Msginterface);
            });

            this.connection.On<string>("response", (response) =>
            {
                //if (response == "accepted")
                //{
                //    this.response.emit(true);
                //}
                //else
                //{
                //    this.response.emit(false);
                //}

            });

            this.connection.On<string>("peerHasLeft", (peerClient) =>
            {
                Debug.WriteLine(peerClient + " left the room");
                Peering peering = localClient.getPeeringByPeerClient(peerClient);
                var removeVideo = remoteVideo.FirstOrDefault(x => x.id == peering.generatedId);
                videoList.Children.Remove(removeVideo.remote);
                localClient.peerings.Remove(peering);
                //var localclient = this.CheckStream("camera")
                //localclient.deletePeeringWith(peerClient);
            });


            this.connection.On<object>("allLoggedUsers", (allUsers) =>
            {
                //this.allUsers.emit(allUsers);
            });

            this.connection.On<string, string>("newUser", (name, email) =>
            {
                //const Msginterface: IMsgInterface = { name: name, msg: email }
                //this.newUser.emit(Msginterface);
            });

            this.connection.On<string, string>("leftUser", (name, email) =>
            {
                //const Msginterface: IMsgInterface = { name: name, msg: email }
                //this.leftUser.emit(Msginterface);
            });

            this.connection.On<object>("allUsers", (allUsers) =>
            {
                Debug.WriteLine("Users in the group" + allUsers);
            });

            this.connection.On<string, string>("receiveMsg", (UserName, Msg) =>
            {
                //const msg: IMsgInterface = { name: UserName, msg: Msg }
                //this.messageReceived.emit(msg);
            });

            this.connection.On<List<UserInRoom>>("GetAllActiveConnectionsInRoomAsync", async (allCurrentUserInRoom) =>
            {
                //List<UserInRoom> allCurrentUserInRoom = new List<UserInRoom>();
                //allCurrentUserInRoom = allcurrentuserInRoom;
                Debug.WriteLine("all users" + allCurrentUserInRoom);

                // send offer for each person already in the room
                foreach (var user in allCurrentUserInRoom)
                {
                    string connectionID = user.ConnectionId;
                    var remote = new MediaElement();
                    remote.Height = 400;
                    remote.Width = 400;
                    videoList.Children.Add(remote);
                    var peering = new Peering(connectionID, connection, remote, Window.Current.Dispatcher);
                    remote.Name = peering.generatedId;
                    peering.createPeerConnection(connectionID, localClient.allStream);

                    remoteVideo.Add(new remoteVideo { remote = remote, id = peering.generatedId });

                    // userName(user["ConnectionId"]);
                    var offer = await peering.createOffer();
                    localClient.peerings.Add(peering);

                    await connection.InvokeAsync("SendOffer", JsonConvert.SerializeObject(offer), connectionID, "windows");
                    Debug.WriteLine("STEP 2: offer sended.");
                }
            });
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //await InitializeHub();
            // Request access to microphone and camera
            var settings = new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
            var capture = new MediaCapture();
            await capture.InitializeAsync(settings);

            // Retrieve a list of available video capture devices (webcams).
            IReadOnlyList<VideoCaptureDevice> deviceList =
            await DeviceVideoTrackSource.GetCaptureDevicesAsync();

            // Get the device list and, for example, print them to the debugger console
            foreach (var device in deviceList)
            {
                // This message will show up in the Output window of Visual Studio
                Debug.WriteLine($"Webcam {device.name} (id: {device.id})\n");
            }

            //    _peerConnection = new PeerConnection();

            //    var config = new PeerConnectionConfiguration
            //    {
            //        IceServers = new List<IceServer> {
            //    new IceServer{ Urls = { "stun:bn-turn1.xirsys.com" } },
            //    new IceServer{ TurnUserName = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=", TurnPassword= "aad634b4-2e25-11eb-81c0-0242ac140004", Urls= {"turn:bn-turn1.xirsys.com:80?transport=udp", "turn:bn-turn1.xirsys.com:3478?transport=udp", "turn:bn-turn1.xirsys.com:80?transport=tcp", "turn:bn-turn1.xirsys.com:3478?transport=tcp", "turns:bn-turn1.xirsys.com:443?transport=tcp", "turns:bn-turn1.xirsys.com:5349?transport=tcp" } }
            //}
            //    };
            //    await _peerConnection.InitializeAsync(config);

            Debug.WriteLine("Peer connection initialized successfully.\n");

        }

        private async void InitilizeHub()
        {
            connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/peeringHub")
            .WithAutomaticReconnect()
            .Build();

            await connection.StartAsync();
            await this.connection.InvokeAsync("GetConnectionIdUWP");

        }



        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            if (_peerConnection != null)
            {
                _peerConnection.Close();
                _peerConnection.Dispose();
                _peerConnection = null;
            }
        }

        public string GetUntilOrEmpty(string text, string stopAt = "typ")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public void SelfVideo_MediaFailed(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SelfVideo_MediaFailed");

        }

        async void invokable_initVideoconference(string stream)
        {
            await connection.InvokeAsync("Connect", Int64.Parse(roomId), name, "Granted", stream, name);
            //ContentRoot.Visibility = Visibility.Collapsed;
            Debug.WriteLine("Room Name" + roomId + name);
            // Creating local profil and start to display own video
            localClient = new LocalClient(Window.Current.Dispatcher);

            if (stream == "camera")
            {
                ContentRoot.Visibility = Visibility.Collapsed;
                //mediaElement = element;
                element = myself;
                localClient.localVideo = element;
                //SelfVideo = localClient.localVideo;
                allLocalClients.Clear();
                numberOfUsers = 0;


                await localClient.getUserMedia(true);

                await this.connection.InvokeAsync("GetAllActiveConnectionsInRoomAsyncUWP", Int64.Parse(roomId));
                //this.microphone(false);
                //this.camera(false);
                //await localClient.displayname();
            }
            if (stream == "screen")
            {
                allLocalClients.Add(new init { localclient = localClient, stream = stream });
                var Screenstream = await localClient.getUserMedia(false);
                //this.screenStream.emit(Screenstream);

                await this.connection.InvokeAsync("GetAllActiveConnectionsInRoomAsyncUWP", Int64.Parse(roomId));
            }
        }


        //for checking
        LocalClient CheckStream(string stream)
        {
            var localClient = allLocalClients.FirstOrDefault(i => i.stream == stream);
            if (localClient != null)
            {
                LocalClient localclient = (LocalClient)localClient.localclient;
                return localclient;
            }
            else
            {
                return null;
            }
        }

        void removeLocalClient(object obj)
        {
            var localclient = allLocalClients.FirstOrDefault(x => x.localclient == obj);
            allLocalClients.Remove(localclient);
        }



        //Button Functions
        //async void camera(Boolean e)
        //{
        //    var localClient = this.CheckStream("camera");
        //    localClient.videoStatus = e;
        //    foreach (var track in localClient.tracksVideo)
        //    {
        //        track.Stop();
        //    }
        //}

        async void microphone(Boolean e)
        {
            var localClient = this.CheckStream("camera");
            localClient.audioStatus = e;
            foreach (var track in localClient.tracksAudio)
            {
                track.Stop();
            }
        }

        async void leave()
        {
            numberOfUsers = 0;
            var localClient = this.CheckStream("camera");
            localClient.videoStatus = false;
            foreach (var track in localClient.tracks)
            {
                track.Stop();
            }
            await this.connection.InvokeAsync("Leave", Email);

            allLocalClients.Clear();
        }

        async void screenshare(Boolean share)
        {
            if (share == true)
            {
                Debug.WriteLine("login screenshare" + roomId + name + Email);
                this.invokable_initVideoconference("screen");
            }
            if (share == false)
            {
                var localClient = this.CheckStream("screen");
                foreach (var track in localClient.tracksScreen)
                {
                    track.Stop();
                }
                this.removeLocalClient(localClient);
                //for (var i = 0; i < localClient.tracksScreen.length; i++) {
                //    var track = localClient.tracksScreen[i];
                //    track.stop();
                //}
                await this.connection.InvokeAsync("EndScreenShare", Email);
            }
        }


        void chatInit()
        {
            invokable_initVideoconference("chat");
        }


        async void sendMsg(string Msg)
        {
            //const Msginterface: IMsgInterface = { name: UserName, msg: Msg }
            //await this.connection.invoke("MsgSend", Email, Msginterface);
        }





        public Task<ContentDialogResult> MsgBox(string title, string content)
        {
            Task<ContentDialogResult> X = null;

            var msgbox = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            try
            {
                X = msgbox.ShowAsync().AsTask<ContentDialogResult>();
                return X;
            }
            catch
            {
                return null;
            }
        }
        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(password.Password))
            {
                await MsgBox("Hey", "Fill Password");
            }
            else if (string.IsNullOrEmpty(userName.Text))
            {
                await MsgBox("Hey", "Fill Email");
            }
            else
            {
                try
                {
                    var result = await _apiHelper.Authenticate(userName.Text, password.Password);
                    if (result != null)
                    {
                        name = result.name;
                        Email = result.email;
                        ContentRoot.Visibility = Visibility.Collapsed;
                        meetingsGrid.Visibility = Visibility.Visible;
                        GetMeetings();
                    }
                }
                catch (Exception ex)
                {
                    await MsgBox("", "incorrect UserName or Password");
                }
            }
        }

        private async void GetMeetings()
        {
            var meetings = await _apiHelper.GetMeetings();
            meetings.ForEach(x => initiatedMeetings.Add(x));
            //initiatedMeetings = meetings;
        }

        class UserDataCollection : ObservableCollection<Rootobject>
        {
            public UserDataCollection()
            {

            }
        }
        private void Joining(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = (Rootobject)((Button)e.OriginalSource).DataContext;
                var OrgPermission = "Granted";
                roomId = data.id.ToString();
                orgPermission = OrgPermission;
                invokable_initVideoconference("camera");
                meetingsGrid.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message.ToString());
            }
        }

    }



    class LocalClient
    {
        public string roomId;
        string userName;
        public dynamic localStream;
        public MediaElement localVideo { get; set; }

        public MediaStream allStream;
        MediaStream screenStream;
        MediaStreamType stream;
        public dynamic tracks;
        public MediaVideoTrack tracksVideo { get; set; }
        public dynamic tracksAudio { get; set; }
        public dynamic tracksScreen { get; set; }
        public Boolean audioStatus { get; set; }
        public Boolean videoStatus { get; set; }
        public CoreDispatcher _uiDispatcher;


        public object MediaLock { get; set; } = new object();

        //public Peering[] peerings;
        public List<Peering> peerings = new List<Peering>();
        private Media _media;
        private static readonly object InstanceLock = new object();
        public LocalClient(CoreDispatcher _uiDispatcher)
        {
            this._uiDispatcher = _uiDispatcher;

        }
        public async Task<dynamic> getUserMedia(Boolean camera)
        {
            WebRTC.Initialize(_uiDispatcher);
            _media = Media.CreateMedia();


            RTCMediaStreamConstraints constraintsAll = new RTCMediaStreamConstraints()
            {
                audioEnabled = true,
                videoEnabled = true
            };


            try
            {
                if (camera == true)
                {
                    //numberOfUsers++;
                    //videoStream = await media.GetUserMedia(constraintsVideo);
                    //audioStream = await media.GetUserMedia(constraintsAudio);

                    allStream = await _media.GetUserMedia(constraintsAll);

                    this.localStream = allStream.GetVideoTracks();
                    audioStatus = videoStatus = true;
                    //this.localVideo = document.getElementById('localVideo');
                    //this.localVideo.srcObject = this.videoStream;


                    this.tracks = this.allStream.GetTracks();
                    this.tracksAudio = this.allStream.GetAudioTracks();
                    this.tracksVideo = this.allStream.GetVideoTracks().FirstOrDefault();

                    //var localstream = allStream.GetVideoTracks().FirstOrDefault();

                    _media.AddVideoTrackMediaElementPair(tracksVideo, localVideo, "SELF");

                    //EnableLocalVideoStream();
                    //dynamicStyle();
                    return null;
                }
                else
                {
                    this.screenStream = await _media.GetUserMedia(constraintsAll);
                    this.localStream = this.screenStream;
                    this.videoStatus = false;

                    this.tracksScreen = this.screenStream.GetVideoTracks();


                    //this.tracksScreen[0].onended = function () {
                    //    console.log("screenshare stopped");

                    //    // doWhatYouNeedToDo();
                    //};
                    return this.tracksScreen[0];
                }




            }
            catch (Exception exp)
            {
                Debug.WriteLine("Error getting user media." + exp);
                return null;
            }
        }

        public void EnableLocalVideoStream()
        {
            lock (MediaLock)
            {
                if (allStream != null)
                {
                    foreach (MediaVideoTrack videoTrack in allStream.GetVideoTracks())
                    {
                        videoTrack.Enabled = true;
                    }
                }
                //VideoEnabled = true;
            }
        }

        //async Task displayname()
        //{
        //    //declare your username
        //    var para = document.createElement("h3");
        //    var t = document.createTextNode("You");
        //    para.appendChild(t);
        //    document.querySelector('.videoboard').appendChild(para);
        //}


        public Peering getPeeringByPeerClient(string peerClient)
        {
            return peerings.FirstOrDefault(x => x.peerClient == peerClient);
        }

        //async Task deletePeeringWith(dynamic peerClient)
        //{
        //    var peering = this.getPeeringByPeerClient(peerClient);
        //    if (peering != null)
        //    {
        //        var remoteVideoToDelete = document.getElementById(peering.generatedId);
        //        remoteVideoToDelete.remove();
        //        peerings = peerings.Where(x => x.peerClient != peerClient).ToArray();
        //        //    numberOfUsers--;
        //        //dynamicStyle();
        //    }
        //}
    }




    public class Peering
    {
        public string peerClient { get; set; }
        public HubConnection connection;
        public RTCPeerConnection peerConnection;
        MediaElement remoteVideo;
        dynamic remoteStream;
        public string generatedId;
        const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
        Media _media;
        MediaElement remoteElement;
        private CoreDispatcher _uiDispatcher;


        public Peering(dynamic peerClient, HubConnection connection, MediaElement remoteElement, CoreDispatcher _uiDispatcher)
        {
            this._uiDispatcher = _uiDispatcher;
            this.peerClient = peerClient;
            this.connection = connection;
            this.remoteElement = remoteElement;
            //generatedId = new Random().Next().ToString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
            generatedId = pool[new Random().Next(pool.Length)].ToString();
            _media = Media.CreateMedia();
            WebRTC.Initialize(this._uiDispatcher);
        }

        private static List<RTCIceServer> GetDefaultList()
        {
            return new List<RTCIceServer>
            {
                new RTCIceServer
                {
                    Url = "stun:bn-turn1.xirsys.com",
                    Username = string.Empty,
                    Credential = string.Empty
                },
                new RTCIceServer
                {
                    Url = "turn:bn-turn1.xirsys.com:80?transport=udp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"
                },
                new RTCIceServer
                {
                    Url = "turn:bn-turn1.xirsys.com:3478?transport=udp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"                },
                new RTCIceServer
                {
                    Url = "turn:bn-turn1.xirsys.com:80?transport=tcp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"                },
                new RTCIceServer
                {
                    Url = "turn:bn-turn1.xirsys.com:3478?transport=tcp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"                },
                new RTCIceServer
                {
                    Url = "turns:bn-turn1.xirsys.com:443?transport=tcp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"                },
                new RTCIceServer
                {
                    Url = "turns:bn-turn1.xirsys.com:5349?transport=tcp",
                    Username = "w86wwQtu1nJLm9RXofg7tR79KGlq5CdqaZoq7sNJwvihY3huk6P-5tYhxfuv6cNOAAAAAF-8tHdlc2hhbjE2MDQ=",
                    Credential = "aad634b4-2e25-11eb-81c0-0242ac140004"                }
            };
        }

        public void createPeerConnection(string userId, MediaStream allStream)
        {
            var config = new RTCConfiguration()
            {
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = GetDefaultList()
            };
            Debug.WriteLine("turn configuration" + config);
            this.peerConnection = new RTCPeerConnection(config);
            this.peerConnection.OnIceCandidate += this.onDetectIceCandidate;
            this.peerConnection.AddStream(allStream);
            //this.addTracksToPeerConnection(allStream);

            peerConnection.OnAddStream += gotRemoteStream;
            Debug.WriteLine("Connection" + connection);
        }

        public async Task<RTCSessionDescription> createOffer()
        {
            try
            {
                var offer = await this.peerConnection.CreateOffer();
                await this.peerConnection.SetLocalDescription(offer);
                Debug.WriteLine("STEP 1: offer created and inserted as local description.");
                return offer;
            }
            catch (Exception error)
            {
                Debug.WriteLine("ERROR: creating an offer." + error);
                return null;
            }
        }

        public async Task<RTCSessionDescription> createAnswer()
        {
            try
            {
                var answer = await this.peerConnection.CreateAnswer();
                await this.peerConnection.SetLocalDescription(answer);
                Debug.WriteLine("STEP 4: answer created and inserted as local description.");
                return answer;
            }
            catch (Exception error)
            {
                Debug.WriteLine("ERROR: creating an answer." + error);
                return null;
            }
        }

        async void onDetectIceCandidate(RTCPeerConnectionIceEvent _event)
        {
            if (_event.Candidate != null)
            {
                var _candidate = new RTCIceCandidate(_event.Candidate.Candidate, _event.Candidate.SdpMid, _event.Candidate.SdpMLineIndex);
                await connection.InvokeAsync("SendIceCandidate", JsonConvert.SerializeObject(_candidate), peerClient, "windows");
                Debug.WriteLine("Ice candidate send UWP. " + _candidate);
            }
        }

        async Task addIceCandidate(RTCIceCandidate iceCandidate)
        {
            await this.peerConnection.AddIceCandidate(iceCandidate);
        }


        void gotRemoteStream(MediaStreamEvent e)
        {
            this.remoteStream = e.Stream?.GetVideoTracks().FirstOrDefault();
            this._media.AddVideoTrackMediaElementPair(remoteStream, remoteElement, generatedId);
            Debug.WriteLine("Received a remote stream");
        }


    }
}

