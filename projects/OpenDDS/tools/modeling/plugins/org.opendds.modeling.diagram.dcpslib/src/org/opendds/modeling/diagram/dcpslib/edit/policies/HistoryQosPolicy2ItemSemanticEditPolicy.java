/*
 * (c) Copyright Object Computing, Incorporated. 2005,2010. All rights reserved.
 */
package org.opendds.modeling.diagram.dcpslib.edit.policies;

import org.eclipse.emf.ecore.EAnnotation;
import org.eclipse.gef.commands.Command;
import org.eclipse.gmf.runtime.diagram.core.commands.DeleteCommand;
import org.eclipse.gmf.runtime.emf.commands.core.command.CompositeTransactionalCommand;
import org.eclipse.gmf.runtime.emf.type.core.commands.DestroyElementCommand;
import org.eclipse.gmf.runtime.emf.type.core.requests.DestroyElementRequest;
import org.eclipse.gmf.runtime.notation.View;
import org.opendds.modeling.diagram.dcpslib.providers.OpenDDSDcpsLibElementTypes;

/**
 * @generated
 */
public class HistoryQosPolicy2ItemSemanticEditPolicy extends OpenDDSDcpsLibBaseItemSemanticEditPolicy {

	/**
	 * @generated
	 */
	public HistoryQosPolicy2ItemSemanticEditPolicy() {
		super(OpenDDSDcpsLibElementTypes.HistoryQosPolicy_3040);
	}

	/**
	 * Do not really destroy the element since the compartment holds non-containment
	 * references while GMF expects the compartment to hold contained references.
	 * Therefore a DestroyReferenceCommand is returned instead of a DestroyElementCommand.
	 * @generated NOT
	 */
	protected Command getDestroyElementCommand(DestroyElementRequest req) {
		CompositeTransactionalCommand cmd = new CompositeTransactionalCommand(getEditingDomain(), null);
		cmd.setTransactionNestingEnabled(false);
		cmd.add(com.ociweb.gmf.edit.commands.RequestToCommandConverter
				.destroyElementRequestToDestroyReferenceCommand(req, getHost(), getEditingDomain()));
		return getGEFWrapper(cmd);
	}
}
