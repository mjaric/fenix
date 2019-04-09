defmodule SampleServer.Worker do
  use GenServer
  require Logger

  def start_link(opts\\[]) do
    GenServer.start_link(__MODULE__, nil, opts)
  end

  def init(_) do
    {:ok, %{}, timeout()}
  end

  def handle_info(:timeout, s) do
    Logger.debug("Broadcasting mesage")
    {:ok, currentTime} =
      :calendar.local_time()
      |> elem(1)
      |> Time.from_erl()
    SampleServerWeb.Endpoint.broadcast!("room:lobby", "server_time", %{user: "system", message: "Time on server #{currentTime}"})
    {:noreply, s, timeout()}
  end

  defp timeout() do
    # {_, {_, _, s}} = :calendar.local_time()
    # (60 - s) * 1000
    10000
  end
end
