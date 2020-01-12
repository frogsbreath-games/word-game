import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type PlayerTileProps = {
  player: GameStore.Player;
  localPlayer: GameStore.Player;
  key: number;
  code: string;
  swapTeams: (player: GameStore.Player) => void;
  leaveGame: () => void;
};

const PlayerTile = ({
  player,
  localPlayer,
  swapTeams,
  leaveGame,
  code
}: PlayerTileProps) => (
  <div>
    <div className="card">
      <div className="card-body">
        <h5 className="card-title">{player.name}</h5>
        <p className="card-text">
          {player.isSpyMaster ? "Spy Master" : "Agent"}
        </p>
        {(localPlayer.isOrganizer || player.number === localPlayer.number) && (
          <button
            className="btn btn-primary"
            type="button"
            onClick={() => swapTeams(player)}
          >
            Swap Teams
          </button>
        )}
        {player.number == localPlayer.number && !localPlayer.isOrganizer && (
          <button
            className="btn btn-danger"
            type="button"
            onClick={() => leaveGame()}
            style={{ marginLeft: "10px" }}
          >
            Leave Game
          </button>
        )}
      </div>
    </div>
  </div>
);

class Lobby extends React.PureComponent<GameProps> {
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
            <h1 className="text-danger">Red</h1>
            <hr />
            {this.props.game.players
              .filter(player => player.team === "red")
              .map(player => (
                <PlayerTile
                  player={player}
                  localPlayer={this.props.localPlayer}
                  code={this.props.game.code}
                  key={player.number}
                  swapTeams={() => this.swapTeams(player)}
                  leaveGame={() => this.quitGame()}
                />
              ))}
          </div>
          <div className="col-sm-6">
            <h1 className="text-primary">Blue</h1>
            <hr />
            {this.props.game.players
              .filter(player => player.team === "blue")
              .map(player => (
                <PlayerTile
                  player={player}
                  localPlayer={this.props.localPlayer}
                  code={this.props.game.code}
                  key={player.number}
                  swapTeams={() => this.swapTeams(player)}
                  leaveGame={() => this.quitGame()}
                />
              ))}
          </div>
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Lobby as any);
