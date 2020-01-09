import * as React from "react";
import { Route } from "react-router";
import Layout from "./components/Layout";
import Home from "./components/Home";
import GameHome from "./components/GameHome";
import Counter from "./components/Counter";
import FetchData from "./components/FetchData";
import Lobby from "./components/Lobby";
import Game from "./components/Game";

import "./custom.css";

export default () => (
  <Layout>
    <Route exact path="/" component={Home} />
    <Route path="/game" component={Game} />
    <Route path="/game-home" component={GameHome} />
    <Route path="/counter" component={Counter} />
    <Route path="/lobby" component={Lobby} />
    <Route path="/fetch-data/:startDateIndex?" component={FetchData} />
  </Layout>
);
