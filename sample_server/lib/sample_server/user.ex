defmodule SampleServer.User do
  @type t :: %{id: String.t(), username: String.t()}
  @type users :: list(t)

  @users [
    %{id: "1234", username: "foo"},
    %{id: "4321", username: "bar"}
  ]

  @spec all() :: users
  def all() do
    @users
  end

  @spec get(String.t()) :: t | nil
  def get(id) do
    @users
    |> Enum.filter(&(&1.id == id))
    |> hd()
  end

  @spec find_by_username(String.t()) :: t | nil
  def find_by_username(username) do
    @users
    |> Enum.filter(&(&1.username == username))
    |> hd()
  end
end
