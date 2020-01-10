import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
//import { Redirect } from "react-router";
import "./Game.css";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { value: string };

//Make fake tiles for testing
type GameTile = { word: string; team: string; isRevealed: boolean };

var GameTileArray = [] as GameTile[];
//Don't judge this
for (let index = 0; index < 25; index++) {
  let word = index.toString();
  let team = index < 8 ? "red" : "blue";
  if (index > 15) {
    team = "neutral";
  }
  let isRevealed = index % 2 === 0 ? false : true;
  GameTileArray.push({ word, team, isRevealed });
  shuffle(GameTileArray);
}

//blue
var blue = "#009DDC";
//red
var red = "#C3423F";
//tan for bystander
var tan = "#C5AFA4";
//grey for unrevealed
var grey = "#C4C4C4";

//shuffle for test data
function shuffle(array: Array<GameTile>) {
  array.sort(() => Math.random() - 0.5);
}

function getColor(color: string, isRevealed: boolean) {
  if (!isRevealed) {
    return grey;
  }
  switch (color) {
    case "red":
      return red;
    case "blue":
      return blue;
    default:
      return tan;
  }
}

const GameTile = ({ word, team, isRevealed }: GameTile) => (
  <div
    className="word-tile"
    style={{
      textAlign: "center",
      backgroundColor: getColor(team, isRevealed)
    }}
  >
    <div>Word: {word}</div>
    <div>Team: {team}</div>
    <div>Is Revealed: {isRevealed.toString()}</div>
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
    alert("A name was submitted: " + this.state.value);
    event.preventDefault();
  }

  public render() {
    //When game is retrieved redirect to lobby component
    // if (this.props.game.status === "InProgress") {
    //   return <Redirect to="/game-home" />;
    // }
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
            <h2>Guesses Remaining: 1</h2>
          </div>
          <div className="game-board" style={{ marginTop: "10px" }}>
            {GameTileArray.map(tile => (
              <GameTile {...tile} />
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
)(Game as any);
