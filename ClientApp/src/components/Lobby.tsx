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
  key: number;
  swapTeams: (player: GameStore.Player) => void;
};

const PlayerTile = ({ player, swapTeams }: PlayerTileProps) => (
  <div>
    <div className="card">
      <div className="card-body">
        <h5 className="card-title">{player.name}</h5>
        <p className="card-text">
          {player.isSpyMaster ? "Spy Master" : "Agent"}
        </p>
        <button
          className="btn btn-primary"
          type="button"
          onClick={() => swapTeams(player)}
        >
          Swap Teams
        </button>
      </div>
    </div>
  </div>
);

class GameHome extends React.PureComponent<GameProps> {
  public swapTeams(player: GameStore.Player) {
    if (player.team === "blue") {
      player.team = "red";
    } else {
      player.team = "blue";
    }
    this.props.updatePlayer(player);
  }
  public render() {
    //When game is retrieved redirect to lobby component
    if (this.props.game.status !== "lobby") {
      return <Redirect to="/game-home" />;
    }

    return (
      <React.Fragment>
        <h1>Lobby: {this.props.game.code}</h1>
        <div className="row">
          <div className="col-sm-6">
            <h1 className="text-danger">Red</h1>
            <hr />
            {this.props.game.players
              .filter(player => player.team === "red")
              .map((player, index) => (
                <PlayerTile
                  player={player}
                  key={player.number}
                  swapTeams={() => this.swapTeams(player)}
                />
              ))}
          </div>
          <div className="col-sm-6">
            <h1 className="text-primary">Blue</h1>
            <hr />
            {this.props.game.players
              .filter(player => player.team === "blue")
              .map((player, index) => (
                <PlayerTile
                  player={player}
                  key={player.number}
                  swapTeams={() => this.swapTeams(player)}
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
)(GameHome as any);
