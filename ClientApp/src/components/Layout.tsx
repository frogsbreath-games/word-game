import * as React from "react";
import { Container } from "reactstrap";
import NavMenu from "./NavMenu";
import LogoFooter from "./LogoFooter";

export default (props: { children?: React.ReactNode }) => (
  <React.Fragment>
    <NavMenu />
    <Container>{props.children}</Container>
    <LogoFooter />
  </React.Fragment>
);
