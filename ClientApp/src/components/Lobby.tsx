import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import PlayerTile from "./PlayerTile";
import { ReactComponent as MessageIcon } from "../assets/MessageIcon.svg";
import { red, blue } from "../constants/ColorConstants";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { input: string };
class Lobby extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { input: "" };
    this.sendMessage = this.sendMessage.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
    this.handleChange = this.handleChange.bind(this);
  }

  public handleKeyPress(event: React.KeyboardEvent) {
    console.log("Key Pressed");
    var keyCode = event.keyCode || event.which;
    if (keyCode == 13) {
      this.sendMessage();
    }
  }

  public sendMessage() {
    if (this.props.connection) {
      this.props.connection
        .invoke("SendMessage", this.state.input)
        .catch(err => console.error(err));
      this.setState({ input: "" });
    }
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ input: event.target.value });
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public componentDidMount() {
    //game has not been retrieved do not create a connection ever
    if (this.props.game.status) {
      //if we are supposed to be in the lobby and we don't have a connection make one
      if (this.props.game.status === "lobby" && !this.props.connection) {
        this.ensureConnectionExists();
      }
    }
  }

  private ensureConnectionExists() {
    this.props.createConnection();
  }

  public swapTeams(player: GameStore.Player) {
    if (player.team === "blue") {
      player.team = "red";
    } else {
      player.team = "blue";
    }
    this.props.updatePlayer(player);
  }

  public deleteGame(code: string) {
    this.props.deleteGame(code);
  }

  public quitGame() {
    this.props.quitGame();
  }

  public deleteBot(playerNumber: number) {
    this.props.deleteBot(playerNumber);
  }

  public render() {
    if (!this.props.game.status || this.props.game.status !== "lobby") {
      return <Redirect to="/game-home" />;
    }

    let organizerButtons;
    if (this.props.localPlayer && this.props.localPlayer.isOrganizer) {
      organizerButtons = (
        <div className="row mx-auto">
          <button
            disabled={!this.props.game.canStart}
            className="btn btn-primary"
            type="button"
            onClick={() => this.props.startGame(this.props.game.code)}
          >
            Start Game
          </button>
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => this.props.addBot()}
            style={{ marginLeft: "10px" }}
          >
            Add Bot
          </button>
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => this.props.requestCurrentGame()}
            style={{ marginLeft: "10px" }}
          >
            Refresh
          </button>
          <button
            className="btn btn-danger"
            type="button"
            onClick={() => this.props.deleteGame(this.props.game.code)}
            style={{ marginLeft: "10px" }}
          >
            Delete Game
          </button>
        </div>
      );
    }

    return (
      <React.Fragment>
        <h1>Lobby: {this.props.game.code}</h1>
        {organizerButtons}
        <div className="row">
          <div className="col-sm-6">
            <h1 style={{ color: red }}>Red</h1>
            <hr />
            {this.props.game.players &&
              this.props.game.players
                .filter(player => player.team === "red")
                .map(player => (
                  <PlayerTile
                    player={player}
                    localPlayer={this.props.localPlayer}
                    code={this.props.game.code}
                    key={player.number}
                    swapTeams={() => this.swapTeams(player)}
                    leaveGame={() => this.quitGame()}
                    deleteBot={() => this.deleteBot(player.number)}
                  />
                ))}
          </div>
          <div className="col-sm-6">
            <h1 style={{ color: blue }}>Blue</h1>
            <hr />
            {this.props.game.players &&
              this.props.game.players
                .filter(player => player.team === "blue")
                .map(player => (
                  <PlayerTile
                    player={player}
                    localPlayer={this.props.localPlayer}
                    code={this.props.game.code}
                    key={player.number}
                    swapTeams={() => this.swapTeams(player)}
                    leaveGame={() => this.quitGame()}
                    deleteBot={() => this.deleteBot(player.number)}
                  />
                ))}
          </div>
        </div>
        <div className="row" style={{ marginTop: "20px" }}>
          <div className="input-group mb-3">
            <div className="input-group-prepend">
              <span className="input-group-text" id="inputGroup-sizing-default">
                <MessageIcon
                  style={{
                    opacity: 0.5,
                    height: "24px",
                    fill: "black"
                  }}
                />
              </span>
            </div>
            <input
              className="form-control"
              type="text"
              placeholder="Message something..."
              value={this.state.input}
              onChange={this.handleChange}
              onKeyPress={this.handleKeyPress}
            />
          </div>
        </div>
        {this.props.messages &&
          this.props.messages.map(message => (
            <span>{message.name + ": " + message.message}</span>
          ))}
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Lobby as any);
