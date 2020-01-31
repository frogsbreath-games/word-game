import * as React from "react";
import { Container } from "reactstrap";
import LogoFooter from "./LogoFooter";
import NavMenu from "./NavMenu";

export default (props: { children?: React.ReactNode }) => (
  <React.Fragment>
    <NavMenu />
    {props.children}
  </React.Fragment>
);
