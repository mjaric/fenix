import {
  Socket,
  Presence
} from "phoenix";

let socket = new Socket("/socket", {
  params: {
    token: window.userToken
  }
});


socket.connect();

// Now that you are connected, you can join channels with a topic:
let channel = socket.channel("room:lobby", {
  random_number: Math.round(Math.random() * 1000)
});

let channel2 = socket.channel("room:lobby", {
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

setTimeout(() => {
  channel2
    .join()
    .receive("ok", (response) => console.info("second join========", response))
    .receive("error", (response) => console.error("second join========", response))
    .receive("timeout", () => console.warn("second join========", "timeout"));
}, 10000);



export default socket;