import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import styles from "./Game.module.css";
import { ReactComponent as RevealedIcon } from "../assets/RevealedIcon.svg";
import { ReactComponent as TileOuter } from "../assets/TileOuter.svg";
import { ReactComponent as TileInner } from "../assets/TileInner.svg";
import { ReactComponent as ConfirmIcon } from "../assets/CheckIcon.svg";
import { ReactComponent as CancelIcon } from "../assets/CancelIcon.svg";
import { ReactComponent as SkullIcon } from "../assets/SkullIcon.svg";
import CharacterImg from "./CharacterImg";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { hintWord: string; wordCount: number; input: string };

type TeamTileProps = {
  team: GameStore.Team;
  tilesRemaining: number;
  isTeamsTurn: boolean;
  players: GameStore.Player[];
};

const TeamTile = ({
  team,
  tilesRemaining,
  isTeamsTurn,
  players,
}: TeamTileProps) => (
  <div className={team === "red" ? styles.redTile : styles.blueTile}>
    <h3 className={team === "red" ? styles.redTeamLabel : styles.blueTeamLabel}>
      {team + " Team"}
    </h3>
    <div>
      <h3>Remaining Tiles: {tilesRemaining}</h3>
      {isTeamsTurn && <h6 className={styles.lightLabel}>Current Turn</h6>}
    </div>
    {players.map((player) => (
      <div
        className={team === "red" ? styles.redTeamLabel : styles.blueTeamLabel}
      >
        {player.type +
          " " +
          player.name +
          " " +
          (player.type === "cultist" ? " 👿" : " 🤠")}
      </div>
    ))}
  </div>
);

type PlayerTrackerProps = {
  player: GameStore.Player;
  team: GameStore.Team;
};

const PlayerTracker = ({ player, team }: PlayerTrackerProps) => (
  <div className={styles.tracker}>
    <div className={styles.character}>
      <CharacterImg number={player.characterNumber} />
    </div>
    <h4 className={team === "red" ? styles.redHeader : styles.blueHeader}>
      {player.name}
    </h4>
  </div>
);

type GameTileProps = {
  wordTile: GameStore.WordTile;
  localPlayer: GameStore.Player;
  turnStatus: string;
  handleClickWord: (word: string, isRevealed: boolean) => void;
};

const GameTile = ({
  wordTile,
  localPlayer,
  turnStatus,
  handleClickWord,
}: GameTileProps) => (
  <div
    className={styles.wordTile}
    onClick={() => handleClickWord(wordTile.word, wordTile.isRevealed)}
  >
    <div className={styles.tileContainer}>
      <TileOuter className={styles[wordTile.team + "TileOuter"]} />
      <TileInner className={styles[wordTile.team + "TileInner"]} />
      <div className={styles.absoluteCenter}>
        {!wordTile.isRevealed && (
          <h5 className={styles.wordTileWord}>{wordTile.word}</h5>
        )}
        {wordTile.votes.map((playerVote) => (
          <div
            key={playerVote.number}
            className={
              (playerVote.team === "red" ? styles.redVote : styles.blueVote) +
              " " +
              (turnStatus === "tallying" ? styles.pulsing : styles.pulse)
            }
          />
        ))}
        {turnStatus === "boardReview" && localPlayer.role === "organizer" && (
          <h3 className={styles.replace} title="Replace Word">
            &#8635;
          </h3>
        )}
      </div>
      {wordTile.isRevealed && (
        <div className={styles.absoluteCenter}>
          {wordTile.team === "black" ? (
            <SkullIcon className={styles[wordTile.team + "Reveal"]} />
          ) : (
            <RevealedIcon className={styles[wordTile.team + "Reveal"]} />
          )}
        </div>
      )}
    </div>
  </div>
);

class Game extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { hintWord: "", wordCount: 0, input: "" };

    this.handleWordChange = this.handleWordChange.bind(this);
    this.handleCountChange = this.handleCountChange.bind(this);
    this.sendMessage = this.sendMessage.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleSubmitClick = this.handleSubmitClick.bind(this);
    this.handleClickWord = this.handleClickWord.bind(this);
  }

  public componentDidMount() {
    //game has not been retrieved do not create a connection ever
    if (this.props.game.status) {
      //if we are supposed to be in the lobby and we don't have a connection make one
      if (
        (this.props.game.status === "inProgress" ||
          this.props.game.status === "boardReview") &&
        !this.props.connection
      ) {
        this.ensureConnectionExists();
      }
    }
  }

  private ensureConnectionExists() {
    this.props.createConnection();
  }

  handleKeyPress(event: React.KeyboardEvent) {
    var keyCode = event.keyCode || event.which;
    if (keyCode === 13) {
      console.log(this.state.input);
      this.sendMessage();
    }
  }

  sendMessage() {
    if (this.props.connection) {
      this.props.connection
        .invoke("SendMessage", this.state.input)
        .catch((err) => console.error(err));
      this.setState({ input: "" });
    }
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ input: event.target.value });
  }

  handleWordChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ hintWord: event.target.value });
  }

  handleCountChange(event: React.ChangeEvent<HTMLSelectElement>) {
    this.setState({ wordCount: parseInt(event.target.value) });
  }

  handleSubmitClick() {
    this.props.giveHint({
      hintWord: this.state.hintWord,
      wordCount: this.state.wordCount,
    } as GameStore.Hint);
    this.setState({
      hintWord: "",
      wordCount: 0,
    });
  }

  handleClickWord(word: string, isRevealed: boolean) {
    if (this.props.game.actions.canReplaceWord) {
      this.props.replaceWord({ word: word });
    }
    //ensure user is allowed to vote and not casting vote for revealed tile
    if (this.props.game.actions.canVote && !isRevealed) {
      this.props.voteWord({ word: word });
    }
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public render() {
    if (
      !this.props.game.status ||
      (this.props.game.status !== "inProgress" &&
        this.props.game.status !== "boardReview")
    ) {
      return <Redirect to="/game-home" />;
    }
    let currentTeam;

    let localTeam = this.props.game.localPlayer.team;
    let opposingTeam = GameStore.GetOpponent(localTeam);
    //probably a better way to do this?
    let localTeamsTilesRemaining = GameStore.GetTilesRemaining(
      this.props.game,
      localTeam
    );

    let opposingTeamTilesRemaining = GameStore.GetTilesRemaining(
      this.props.game,
      opposingTeam
    );

    let currentStatus: string = "boardReview"; //this is a bit of a hack
    let hintWord;
    let wordCount;
    let guessesRemaining;
    if (this.props.game.currentTurn) {
      currentTeam = this.props.game.currentTurn.team;
      currentStatus = this.props.game.currentTurn.status;
      hintWord = this.props.game.currentTurn.hintWord;
      wordCount = this.props.game.currentTurn.wordCount;
      guessesRemaining = this.props.game.currentTurn.guessesRemaining;
    }

    return (
      <React.Fragment>
        <div className={styles.gameBody}>
          <div className={styles.leftSection}>
            <div className={styles.banner}>
              <div className={styles.instructions}>
                <div style={{ textAlign: "center" }}>
                  <h3>{this.props.game.descriptions.status}</h3>
                  <hr />
                  <h6>{this.props.game.descriptions.statusDescription}</h6>
                  <p>{this.props.game.descriptions.localPlayerInstruction}</p>
                </div>
              </div>
              <TeamTile
                team={localTeam}
                tilesRemaining={localTeamsTilesRemaining}
                isTeamsTurn={currentTeam === localTeam}
                players={this.props.game.players.filter(
                  (player) => player.team === localTeam
                )}
              />
              <TeamTile
                team={opposingTeam}
                tilesRemaining={opposingTeamTilesRemaining}
                isTeamsTurn={currentTeam === opposingTeam}
                players={this.props.game.players.filter(
                  (player) => player.team === opposingTeam
                )}
              />

              <div className="row">
                {this.props.game.actions.canDelete && (
                  <button
                    className={styles.cancel}
                    type="button"
                    onClick={() => this.props.deleteGame(this.props.game.code)}
                    style={{ marginTop: "10px", marginLeft: "20px" }}
                  >
                    Delete Game
                  </button>
                )}
                {this.props.game.actions.canRestart && (
                  <button
                    className={styles.submit}
                    type="button"
                    onClick={() => this.props.backToLobby(this.props.game.code)}
                    style={{ marginTop: "10px", marginLeft: "20px" }}
                  >
                    Back To Lobby
                  </button>
                )}
                {this.props.game.actions.canStart && (
                  <button
                    className={styles.submit}
                    type="button"
                    onClick={() => this.props.startGame(this.props.game.code)}
                    style={{ marginTop: "10px", marginLeft: "20px" }}
                  >
                    Start Game
                  </button>
                )}
              </div>
            </div>
          </div>
          <main className={styles.main}>
            <div>
              <div style={{ textAlign: "center", minHeight: "90px" }}>
                {/* this should only be seen by cultist when it is hint phase */}
                {this.props.game.actions.canGiveHint && (
                  <div className={styles.hintInputs}>
                    <h3>Submit Hint & Clue Count</h3>
                    <div className={styles.inputs}>
                      <input
                        type="text"
                        value={this.state.hintWord}
                        className={styles.input}
                        onChange={this.handleWordChange}
                        placeholder="Enter hint here..."
                      />
                      <select
                        value={this.state.wordCount}
                        className={styles.input}
                        onChange={this.handleCountChange}
                      >
                        {Array.from(
                          { length: localTeamsTilesRemaining + 1 },
                          (v, i) => i
                        ).map((number) => (
                          <option key={number} value={number}>
                            {number}
                          </option>
                        ))}
                      </select>
                      <button
                        className={styles.submitHint}
                        type="button"
                        onClick={this.handleSubmitClick}
                      >
                        Submit
                      </button>
                    </div>
                  </div>
                )}
                {this.props.game.actions.canApproveHint && (
                  <div>
                    <h3>Pending hint: "{hintWord}"</h3>
                    <button
                      type="button"
                      className={styles.confirm}
                      style={{ width: 150 }}
                      onClick={() => {
                        this.props.approveHint();
                      }}
                    >
                      <ConfirmIcon width={35} style={{ fill: "black" }} />
                    </button>
                    <button
                      type="button"
                      className={styles.cancel}
                      style={{ width: 150, marginLeft: "20px" }}
                      onClick={() => {
                        this.props.refuseHint();
                      }}
                    >
                      <CancelIcon width={35} style={{ fill: "white" }} />
                    </button>
                  </div>
                )}
                {(currentStatus === "guessing" ||
                  currentStatus === "tallying") && (
                  <div>
                    <h1>
                      Hint: "{hintWord}" ({wordCount})
                    </h1>
                    <h5>
                      {guessesRemaining
                        ? guessesRemaining + " guesses remaining"
                        : "Unlimited guesses remaining"}
                    </h5>
                  </div>
                )}
              </div>
              <div className={styles.board}>
                {this.props.game.wordTiles &&
                  this.props.game.wordTiles.map((tile) => (
                    <GameTile
                      wordTile={tile}
                      key={tile.word}
                      localPlayer={this.props.game.localPlayer}
                      handleClickWord={this.handleClickWord}
                      turnStatus={currentStatus}
                    />
                  ))}
              </div>
              <div style={{ textAlign: "center" }}>
                {this.props.game.actions.canVote && (
                  <button
                    className={styles.submit}
                    type="button"
                    onClick={() => this.props.voteEndTurn()}
                    style={{ marginTop: "10px" }}
                  >
                    End Turn (
                    {this.props.game.currentTurn
                      ? this.props.game.currentTurn.endTurnVotes.length
                      : 0}
                    )
                  </button>
                )}
              </div>
            </div>
          </main>
          <div className={styles.rightSection}>
            <PlayerTracker
              team={localTeam}
              player={this.props.game.localPlayer}
            />
            <div className={styles.chat}>
              <div className={styles.eventWindow}>
                {this.props.events &&
                  this.props.events.map((event, index) => (
                    <div key={index}>
                      <div style={{ margin: "2px" }}>
                        <span
                          style={{
                            fontSize: "10px",
                            fontFamily: "monospace",
                          }}
                        >
                          {event.timestamp}
                        </span>
                        <br />
                        <span className={styles[event.team]}>
                          {event.player ? event.player : "Team " + event.team}
                        </span>
                        <span>{" " + event.message}</span>
                      </div>
                    </div>
                  ))}
              </div>
              <div className={styles.chatInput}>
                <input
                  className={styles.messageInput}
                  type="text"
                  placeholder="Message something..."
                  value={this.state.input}
                  onChange={this.handleChange}
                  onKeyPress={this.handleKeyPress}
                />
              </div>
            </div>
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
