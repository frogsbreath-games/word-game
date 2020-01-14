import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import "./Game.css";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { value: string };
//blue
var blue = "#009DDC";
//red
var red = "#C3423F";
//tan for bystander
var tan = "#C5AFA4";
//grey for unrevealed
var grey = "#C4C4C4";

var black = "#2A2A2A";

function getColor(color: string, isRevealed: boolean) {
  if (!isRevealed) {
    return grey;
  }
  switch (color) {
    case "red":
      return red;
    case "blue":
      return blue;
    case "neutral":
      return tan;
    default:
      return black;
  }
}

type GameTypeProps = {
  wordTile: GameStore.WordTile;
  localPlayer: GameStore.Player;
};
const GameTile = ({ wordTile, localPlayer }: GameTypeProps) => (
  <div
    className="word-tile"
    style={{
      textAlign: "center",
      backgroundColor: getColor(
        wordTile.team,
        localPlayer.isSpyMaster || wordTile.isRevealed
      ),
      padding: "10px"
    }}
  >
    <h6
      style={{
        backgroundColor: "rgba(255, 255, 255, 0.75)",
        padding: "5px",
        borderRadius: "5px",
        boxShadow:
          "0 1px 3px rgba(255, 255, 255, 0.24), 0 1px 3px rgba(255, 255, 255, 0.36)"
      }}
    >
      {wordTile.word}
    </h6>
  </div>
);

class Game extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { value: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ value: event.target.value });
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public render() {
    if (!this.props.game.status || this.props.game.status !== "inProgress") {
      return <Redirect to="/game-home" />;
    }

    return (
      <React.Fragment>
        <div>
          <div style={{ textAlign: "center" }}>
            <h1
              style={{
                color: getColor("red", true)
              }}
            >
              Current Team: Red
            </h1>
          </div>
          <div className="game-board" style={{ marginTop: "10px" }}>
            {this.props.game.wordTiles &&
              this.props.game.wordTiles.map(tile => (
                <GameTile
                  wordTile={tile}
                  key={tile.word}
                  localPlayer={this.props.localPlayer}
                />
              ))}
          </div>
          {this.props.localPlayer.isOrganizer && (
            <div className="row">
              <button
                className="btn btn-danger"
                type="button"
                onClick={() => this.props.deleteGame(this.props.game.code)}
                style={{ marginTop: "10px", marginLeft: "20px" }}
              >
                Delete Game
              </button>
            </div>
          )}
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Game as any);
