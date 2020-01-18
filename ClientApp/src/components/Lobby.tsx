import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

//blue
var blue = "#009DDC";
//red
var red = "#C3423F";
//tan for bystander
var tan = "#C5AFA4";
//grey for unrevealed
var grey = "#C4C4C4";

type PlayerTileProps = {
  player: GameStore.Player;
  localPlayer: GameStore.Player;
  key: number;
  code: string;
  swapTeams: (player: GameStore.Player) => void;
  leaveGame: () => void;
  deleteBot: (playerNumber: number) => void;
};

const PlayerTile = ({
  player,
  localPlayer,
  swapTeams,
  leaveGame,
  deleteBot,
  code
}: PlayerTileProps) => (
  <div>
    <div
      style={{
        borderRadius: "3px",
        margin: "15px",
        padding: "10px",
        backgroundColor: player.team === "red" ? red : blue,
        boxShadow:
          "0 1px 3px rgba(0, 0, 0, 0.24), 0 1px 3px rgba(0, 0, 0, 0.36)"
      }}
    >
      <div>
        <div
          style={{
            backgroundColor: "rgba(255, 255, 255, 0.75)",
            padding: "10px",
            borderRadius: "5px",
            boxShadow:
              "0 1px 3px rgba(255, 255, 255, 0.24), 0 1px 3px rgba(255, 255, 255, 0.36)"
          }}
        >
          <h5>{player.name}</h5>
          {player.number === localPlayer.number && (
            <PlayerIcon
              //Need to know how to do this better
              className="justify-content-end"
              style={{
                opacity: 0.5,
                width: "50px",
                float: "right",
                marginTop: "-25px",
                fill: player.team === "red" ? red : blue
              }}
            />
          )}
          <p>{player.isSpyMaster ? "Spy Master" : "Agent"}</p>
        </div>
        {(localPlayer.isOrganizer || player.number === localPlayer.number) && (
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => swapTeams(player)}
            style={{ marginTop: "10px" }}
          >
            <SwapIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
          </button>
        )}
        {localPlayer.isOrganizer && player.isBot && (
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => deleteBot(player.number)}
            style={{ marginTop: "10px", marginLeft: "5px" }}
          >
            <TrashIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
          </button>
        )}
        {player.number == localPlayer.number && !localPlayer.isOrganizer && (
          <button
            className="btn btn-danger"
            type="button"
            onClick={() => leaveGame()}
            style={{ marginLeft: "10px", marginTop: "10px" }}
          >
            Leave Game
          </button>
        )}
      </div>
    </div>
  </div>
);

type State = { input: string };
class Lobby extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { input: "" };
    this.sendMessage = this.sendMessage.bind(this);
    this.handleChange = this.handleChange.bind(this);
  }

  public sendMessage() {
    debugger;
    console.log(this.state.input);
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
        <div className="row">
          <input
            type="text"
            value={this.state.input}
            onChange={this.handleChange}
          />

          <button onClick={() => this.sendMessage()}>Send</button>
          {this.props.messages &&
            this.props.messages.map(message => (
              <span>{message.name + ": " + message.message}</span>
            ))}
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Lobby as any);
