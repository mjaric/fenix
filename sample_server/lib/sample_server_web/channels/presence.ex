defmodule SampleServerWeb.Presence do
  use Phoenix.Presence,
    otp_app: :sample_server,
    pubsub_server: SampleServer.PubSub
end
