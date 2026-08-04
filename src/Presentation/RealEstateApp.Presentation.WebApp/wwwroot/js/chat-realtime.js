// Real-time Chat using ASP.NET Core SignalR
(function () {
    window.RealEstateChat = {
        connection: null,
        propertyId: 0,
        recipientId: "",
        currentUserId: "",

        init: function (options) {
            this.propertyId = options.propertyId;
            this.recipientId = options.recipientId;
            this.currentUserId = options.currentUserId;

            if (typeof signalR === "undefined") {
                console.warn("SignalR library not loaded. Falling back to HTTP polling.");
                return;
            }

            var self = this;
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/chat")
                .withAutomaticReconnect([0, 2000, 5000, 10000])
                .build();

            this.connection.on("ReceiveMessage", function (msg) {
                if (typeof window.onNewChatMessageReceived === "function") {
                    window.onNewChatMessageReceived(msg);
                }
            });

            this.connection.on("UserTyping", function (data) {
                if (typeof window.onUserTypingStatus === "function") {
                    window.onUserTypingStatus(data);
                }
            });

            this.connection.start()
                .then(function () {
                    console.log("Connected to ChatHub via SignalR");
                    self.connection.invoke("JoinThread", self.propertyId, self.recipientId)
                        .catch(err => console.error(err));
                })
                .catch(function (err) {
                    console.error("SignalR Connection Error: ", err);
                });
        },

        sendMessage: function (messageContent) {
            if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
                return this.connection.invoke("SendMessage", this.propertyId, this.recipientId, messageContent);
            }
            return Promise.reject("Not connected to SignalR hub");
        },

        sendTyping: function (isTyping) {
            if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
                this.connection.invoke("SendTyping", this.propertyId, this.recipientId, isTyping)
                    .catch(err => console.error(err));
            }
        }
    };
})();
