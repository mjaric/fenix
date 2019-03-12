// NOTE: The contents of this file will only be executed if
// you uncomment its entry in "assets/js/app.js".

// To use Phoenix channels, the first step is to import Socket
// and connect at the socket path in "lib/web/endpoint.ex":
import {
  Socket,
  Presence
} from "phoenix";

let socket = new Socket("/socket", {
  params: {
    token: window.userToken
  }
});

// When you connect, you'll often need to authenticate the client.
// For example, imagine you have an authentication plug, `MyAuth`,
// which authenticates the session and assigns a `:current_user`.
// If the current user exists you can assign the user's token in
// the connection for use in the layout.
//
// In your "lib/web/router.ex":
//
//     pipeline :browser do
//       ...
//       plug MyAuth
//       plug :put_user_token
//     end
//
//     defp put_user_token(conn, _) do
//       if current_user = conn.assigns[:current_user] do
//         token = Phoenix.Token.sign(conn, "user socket", current_user.id)
//         assign(conn, :user_token, token)
//       else
//         conn
//       end
//     end
//
// Now you need to pass this token to JavaScript. You can do so
// inside a script tag in "lib/web/templates/layout/app.html.eex":
//
//     <script>window.userToken = "<%= assigns[:user_token] %>";</script>
//
// You will need to verify the user token in the "connect/2" function
// in "lib/web/channels/user_socket.ex":
//
//     def connect(%{"token" => token}, socket) do
//       # max_age: 1209600 is equivalent to two weeks in seconds
//       case Phoenix.Token.verify(socket, "user socket", token, max_age: 1209600) do
//         {:ok, user_id} ->
//           {:ok, assign(socket, :user, user_id)}
//         {:error, reason} ->
//           :error
//       end
//     end
//
// Finally, pass the token on connect as below. Or remove it
// from connect if you don't care about authentication.

socket.connect();

// Now that you are connected, you can join channels with a topic:
let channel = socket.channel("room:lobby", {
  random_number: Math.round(Math.random() * 1000)
});

let presences = {};

channel.on("presence_state", state => {
  presences = Presence.syncState(presences, state);
  renderOnlineUsers(presences);
});

channel.on("presence_diff", diff => {
  presences = Presence.syncDiff(presences, diff);
  renderOnlineUsers(presences);
});

function renderOnlineUsers(presence) {
  let response = "";
  Presence.list(presences, (id, {
    metas: [first, ...rest]
  }) => {
    let count = rest.length + 1;
    response += `<li>${id} (count: ${count})</li>`;
  });

  document.getElementById("users").innerHTML = response;
}

window.d_channel = channel;

let chatInput = document.querySelector("#chat-input");
let messagesContainer = document.querySelector("#messages");
let statusInfo = document.getElementById("statusInfo");
chatInput
  .addEventListener("keypress", event => {
    if (event.keyCode === 13) {
      channel
        .push("new_msg", {
          body: chatInput.value
        })
        .receive("ok", console.info.bind(console))
        .receive("error", console.error.bind(console));
      chatInput.value = "";
    }
  });

channel
  .on("new_msg", payload => {
    let messageItem = document.createElement("li");
    let messageText = document.createElement("div");
    let messageSender = document.createElement("div");
    messageItem.classList = ["messageItem"];
    messageText.classList = ["messageText"];
    messageSender.classList = ["messageSender"];

    messageItem.appendChild(messageSender);
    messageItem.appendChild(messageText);

    messageSender.innerText = `@${payload.user} - [${formatDate(new Date())}]`;
    messageText.innerText = payload.body;
    messagesContainer.appendChild(messageItem);
  });

function formatDate(date) {
  var monthNames = [
    "January", "February", "March",
    "April", "May", "June", "July",
    "August", "September", "October",
    "November", "December"
  ];

  var day = date.getDate();
  var monthIndex = date.getMonth();
  var year = date.getFullYear();

  return day + ' ' + monthNames[monthIndex] + ' ' + year;
}


function joinChannel() {
  channel
    .join()
    .receive("ok", resp => {
      statusInfo.innerText = "Connected";
      console.log("Joined successfully", resp);
    })
    .receive("error", resp => {
      statusInfo.innerText = "Not Connected";
      console.log("Unable to join", resp);
    })
    .receive("timout", resp => {
      statusInfo.innerText = "Timeout";
      console.log("Unable to join, Timeout", resp);
    });
}

joinChannel();

// setTimeout(() => {
//   channel
//     .leave()
//     .receive("ok", (response) => console.info(response))
//     .receive("error", (response) => console.error(response))
//     .receive("timeout", () => console.warn("timeout"));
// }, 10000);



export default socket;