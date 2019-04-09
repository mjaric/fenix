defmodule SampleServerWeb.PageController do
  use SampleServerWeb, :controller

  def index(conn, _params) do
    render(conn, "index.html")
  end

  def login(conn, %{"username" => username}) do
    current_user = SampleServer.User.find_by_username(username)
    salt = Application.get_env(:sample_server, :token_salt)
    token = Phoenix.Token.sign(conn, salt, current_user.id)

    conn
    |> assign(:user_token, token)
    |> json(%{token: token})
  end
end
