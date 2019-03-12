defmodule SampleServerWeb.RoomChannel do
  use Phoenix.Channel
  require Logger
  alias SampleServer.User
  alias SampleServerWeb.Presence

  def join("room:lobby", _params, %{assigns: %{user_id: user_id}} = socket)
      when user_id != nil do
    user = User.get(user_id)
    send(self(), :after_join)
    {:ok, %{response: "Hi #{user.username}, your are in lobby channel now"}, socket}
    # {:error, %{reason: "unauthorized"}}
  end

  def join("room:" <> _private_room_id, _params, _socket) do
    {:error, %{reason: "unauthorized"}}
  end

  def handle_in("new_msg", %{"body" => "delay"}, %{assigns: %{user_id: user_id}} = socket) do
    user = User.get(user_id)
    Process.send_after(self(), {:delayed, %{body: "delay", user: user.username}}, 2_000)
    {:noreply, socket}
  end

  def handle_in("new_msg", %{"body" => "shutdown_silent"}, socket) do
    {:stop, :shutdown, socket}
  end

  def handle_in("new_msg", %{"body" => "shutdown"}, socket) do
    {:stop, :shutdown, {:error, %{message: "closing channel"}}, socket}
  end

  def handle_in("new_msg", %{"body" => "error"}, socket) do
    {:reply, {:error, %{message: "this is error"}}, socket}
  end

  def handle_in("new_msg", %{"body" => "explode"}, _socket) do
    raise "Forced error"
  end

  def handle_in("new_msg", %{"body" => body}, %{assigns: %{user_id: user_id}} = socket) do
    user = User.get(user_id)

    broadcast!(socket, "new_msg", %{body: body, user: user.username})
    {:reply, {:ok, %{}}, socket}
  end

  def handle_info(:after_join, socket) do
    push(socket, "presence_state", Presence.list(socket))
    meta = %{online_at: inspect(System.system_time(:second))}

    socket
    |> Presence.track(socket.assigns.user_id, meta)
    |> case do
      {:ok, _} -> Logger.info("tracking ....")
      {:error, error} -> Logger.error("Unable to track user #{inspect(error)}")
    end

    {:noreply, socket}
  end

  def handle_info({:delayed, msg}, socket) do
    push(socket, "new_msg", msg)
    {:noreply, socket}
  end
end
