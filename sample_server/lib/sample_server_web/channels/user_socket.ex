defmodule SampleServerWeb.UserSocket do
  use Phoenix.Socket
  require Logger

  ## Channels
  channel("room:*", SampleServerWeb.RoomChannel)

  ## Transports
  transport(:websocket, Phoenix.Transports.WebSocket)
  # transport :longpoll, Phoenix.Transports.LongPoll

  # Socket params are passed from the client and can
  # be used to verify and authenticate a user. After
  # verification, you can put default assigns into
  # the socket that will be set for all channels, ie
  #
  #     {:ok, assign(socket, :user_id, verified_user_id)}
  #
  # To deny connection, return `:error`.
  #
  # See `Phoenix.Token` documentation for examples in
  # performing token verification on connect.

  def connect(%{"token" => token}, socket) do
    Logger.debug("UserSocket: Got Token!! OK")
    with {:ok, user_id} <- Phoenix.Token.verify(socket, Application.get_env(:sample_server, :token_salt), token, max_age: 1_209_600),
         user when user != nil <- SampleServer.User.get(user_id) do
      {:ok, assign(socket, :user_id, user_id)}
    else
      nil ->
        Logger.error("User not found")
        :error

      {:error, reason} ->
        Logger.error("Failed to connect to phoenix user socket due #{inspect(reason)}")
        :error
    end
  end

  def connect(_params, socket) do
    Logger.debug("UserSocket: Ignore No Token")
    {:ok, socket}
  end

  # Socket id's are topics that allow you to identify all sockets for a given user:
  #
  #     def id(socket), do: "user_socket:#{socket.assigns.user_id}"
  #
  # Would allow you to broadcast a "disconnect" event and terminate
  # all active sockets and channels for a given user:
  #
  #     SampleServerWeb.Endpoint.broadcast("user_socket:#{user.id}", "disconnect", %{})
  #
  # Returning `nil` makes this socket anonymous.
  @spec id(Phoenix.Socket.t()) :: String.t() | nil
  def id(socket) do
    case socket.assigns[:user_id] do
      nil ->
        nil

      id ->
        "user_socket:#{id}"
    end
  end
end
