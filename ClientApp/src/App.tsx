import * as React from "react";
import { Route } from "react-router";
import Layout from "./components/Layout";
import GameHome from "./components/GameHome";
import Lobby from "./components/Lobby";
import Game from "./components/Game";

import "./custom.css";

export default () => (
  <Layout>
    <Route exact path="/" component={GameHome} />
    <Route path="/game" component={Game} />
    <Route path="/game-home" component={GameHome} />
    <Route path="/join/:gameCode" component={GameHome} />
    <Route path="/lobby" component={Lobby} />
  </Layout>
);
