defmodule SampleServerWeb.Router do
  use SampleServerWeb, :router
  alias SampleServer.User

  pipeline :browser do
    plug(:accepts, ["html"])
    plug(:fetch_session)
    plug(:fetch_flash)
    plug(:protect_from_forgery)
    plug(:put_secure_browser_headers)
    plug(:authenticate)
    plug(:put_user_token)
  end

  pipeline :api do
    plug(:accepts, ["json"])
  end

  scope "/", SampleServerWeb do
    # Use the default browser stack
    pipe_through(:browser)

    get("/", PageController, :index)

    get("/login", PageController, :login)
  end

  # Other scopes may use custom stacks.
  # scope "/api", SampleServerWeb do
  #   pipe_through :api
  # end

  defp authenticate(conn, _) do
    user =
      conn.params["username"]
      |> User.find_by_username()

    unless user == nil do
      assign(conn, :current_user, user)
    else
      conn
    end
  end

  defp put_user_token(conn, _) do
    if current_user = conn.assigns[:current_user] do
      token = Phoenix.Token.sign(conn, Application.get_env(:sample_server, :token_salt), current_user.id)
      assign(conn, :user_token, token)
    else
      conn
    end
  end
end
